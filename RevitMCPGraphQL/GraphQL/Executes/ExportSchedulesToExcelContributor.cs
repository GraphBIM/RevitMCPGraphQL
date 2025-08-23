using GraphQL;
using GraphQL.Types;
using NPOI.XSSF.UserModel;
using System.IO;

namespace RevitMCPGraphQL.GraphQL.Executes;

internal sealed class ExportSchedulesToExcelContributor : IMutationContributor
{
    public void Register(ObjectGraphType mutation, Func<Document?> getDoc)
    {
        mutation.Field<StringGraphType>("exportSchedulesToExcel")
            .Description("Export one or more schedules to an Excel .xlsx file. Returns the output file path.")
            .Arguments(new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "filePath" },
                new QueryArgument<ListGraphType<IdGraphType>> { Name = "scheduleIds" } // optional; all if not provided
            ))
            .Resolve(ctx =>
            {
                var doc = getDoc();
                if (doc == null) return (string?)null;
                var filePath = ctx.GetArgument<string>("filePath");
                var scheduleIds = ctx.GetArgument<List<long>?>("scheduleIds");

                return RevitDispatcher.Invoke(() =>
                {
                    var schedulesQuery = new FilteredElementCollector(doc)
                        .OfClass(typeof(ViewSchedule))
                        .Cast<ViewSchedule>()
                        .Where(vs => !vs.IsTitleblockRevisionSchedule);

                    if (scheduleIds != null && scheduleIds.Count > 0)
                    {
                        var idSet = new HashSet<long>(scheduleIds);
                        schedulesQuery = schedulesQuery.Where(vs => idSet.Contains(vs.Id.Value));
                    }

                    var workbook = new XSSFWorkbook();

                    foreach (var vs in schedulesQuery)
                    {
                        var sheet = workbook.CreateSheet(string.IsNullOrWhiteSpace(vs.Name) ? $"Schedule_{vs.Id.Value}" : vs.Name);
                        var tableData = vs.GetTableData();
                        var body = tableData?.GetSectionData(SectionType.Body);
                        var headerSection = tableData?.GetSectionData(SectionType.Header);
                        if (body == null) continue;

                        // Build visible fields list via definition
                        var def = vs.Definition;
                        var fields = new List<ScheduleField>();
                        for (int i = 0; i < def.GetFieldCount(); i++)
                        {
                            var f = def.GetField(i);
                            if (f != null && !f.IsHidden) fields.Add(f);
                        }
                        // Header row: use header section text if available, else field name
                        var header = sheet.CreateRow(0);
                        int headerRowIndex = headerSection != null ? Math.Max(0, headerSection.NumberOfRows - 1) : -1;
                        for (int col = 0; col < body.NumberOfColumns; col++)
                        {
                            string name = headerRowIndex >= 0 ? (vs.GetCellText(SectionType.Header, headerRowIndex, col) ?? string.Empty) : string.Empty;
                            if (string.IsNullOrWhiteSpace(name))
                            {
                                name = col < fields.Count ? (fields[col].GetName() ?? $"Col{col}") : $"Col{col}";
                            }
                            header.CreateCell(col).SetCellValue(name);
                        }

                        int outRow = 1;
                        for (int r = 0; r < body.NumberOfRows; r++)
                        {
                            var row = sheet.CreateRow(outRow++);
                            for (int c = 0; c < body.NumberOfColumns; c++)
                            {
                                var val = vs.GetCellText(SectionType.Body, r, c) ?? string.Empty;
                                row.CreateCell(c).SetCellValue(val);
                            }
                        }
                    }

                    // ensure directory exists
                    var dir = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        workbook.Write(fs, leaveOpen: false);
                    }
                    workbook.Close();
                    return filePath;
                });
            });
    }
}
