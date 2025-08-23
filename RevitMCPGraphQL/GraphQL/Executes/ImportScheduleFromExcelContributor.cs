using GraphQL;
using GraphQL.Types;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;

namespace RevitMCPGraphQL.GraphQL.Executes;

internal sealed class ImportScheduleFromExcelContributor : IMutationContributor
{
    public void Register(ObjectGraphType mutation, Func<Document?> getDoc)
    {
        mutation.Field<IntGraphType>("importScheduleFromExcel")
            .Description("Import parameter values from an Excel .xlsx file into elements listed in a schedule. Returns count of updated elements.")
            .Arguments(new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "filePath" },
                new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "scheduleId" }
            ))
            .Resolve(ctx =>
            {
                var doc = getDoc();
                if (doc == null) return 0;
                var filePath = ctx.GetArgument<string>("filePath");
                var scheduleId = ctx.GetArgument<long>("scheduleId");

                return RevitDispatcher.Invoke(() =>
                {
                    if (!File.Exists(filePath)) return 0;

                    var vs = doc.GetElement(new ElementId(scheduleId)) as ViewSchedule;
                    if (vs == null) return 0;

                    int updates = 0;
                    using var t = new Transaction(doc, "Import Schedule from Excel");
                    t.Start();
                    try
                    {
                        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        IWorkbook workbook = new XSSFWorkbook(fs);
                        for (int i = 0; i < workbook.NumberOfSheets; i++)
                        {
                            var sheet = workbook.GetSheetAt(i);
                            if (sheet == null) continue;
                            // Expect header row: ElementId + parameter names
                            var header = sheet.GetRow(0);
                            if (header == null) continue;
                            var headers = new List<string>();
                            for (int c = 0; c <= header.LastCellNum; c++)
                            {
                                var cell = header.GetCell(c);
                                headers.Add(cell?.ToString() ?? string.Empty);
                            }
                            // Get body section to bound columns
                            var tableData = vs.GetTableData();
                            var body = tableData?.GetSectionData(SectionType.Body);
                            if (body == null) continue;

                            for (int r = 1; r <= sheet.LastRowNum; r++)
                            {
                                var row = sheet.GetRow(r);
                                if (row == null) continue;
                                var idCell = row.GetCell(0);
                                if (idCell == null) continue;
                                if (!long.TryParse(idCell.ToString(), out var idValue)) continue;
                                var el = doc.GetElement(new ElementId(idValue));
                                if (el == null) continue;

                                for (int c = 1; c < headers.Count; c++)
                                {
                                    var headerName = headers[c];
                                    if (string.IsNullOrWhiteSpace(headerName) || headerName.Equals("ElementId", StringComparison.OrdinalIgnoreCase)) continue;
                                    var param = el.LookupParameter(headerName);
                                    if (param == null || param.IsReadOnly) continue;
                                    var cell = row.GetCell(c);
                                    var val = cell?.ToString() ?? string.Empty;
                                    if (param.StorageType == StorageType.Integer)
                                    {
                                        if (int.TryParse(val, out var iv)) { if (param.Set(iv)) updates++; }
                                    }
                                    else if (param.StorageType == StorageType.Double)
                                    {
                                        if (double.TryParse(val, out var dv)) { if (param.Set(dv)) updates++; }
                                    }
                                    else if (param.StorageType == StorageType.String)
                                    {
                                        if (param.Set(val)) updates++;
                                    }
                                    else
                                    {
                                        // fallback try string
                                        if (param.Set(val)) updates++;
                                    }
                                }
                            }
                        }
                        t.Commit();
                    }
                    catch
                    {
                        t.RollBack();
                        updates = 0;
                    }

                    return updates;
                });
            });
    }
}
