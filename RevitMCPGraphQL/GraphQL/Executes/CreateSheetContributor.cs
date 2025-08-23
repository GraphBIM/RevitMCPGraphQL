using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using GraphQL;
using GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Executes;

internal sealed class CreateSheetContributor : IMutationContributor
{
    public void Register(ObjectGraphType mutation, Func<Document?> getDoc)
    {
        mutation.Field<IdGraphType>("createSheet")
            .Description("Create a sheet (optionally with title block type). Returns the new sheet Id or null.")
            .Arguments(new QueryArguments(
                new QueryArgument<IdGraphType> { Name = "titleBlockTypeId" },
                new QueryArgument<StringGraphType> { Name = "sheetNumber" },
                new QueryArgument<StringGraphType> { Name = "sheetName" }
            ))
            .Resolve(ctx =>
            {
                var doc = getDoc();
                if (doc == null) return (object?)null;
                var tbId = ctx.GetArgument<long?>("titleBlockTypeId");
                var number = ctx.GetArgument<string?>("sheetNumber");
                var name = ctx.GetArgument<string?>("sheetName");
                return RevitDispatcher.Invoke<object?>(() =>
                {
                    using var t = new Transaction(doc, "Create Sheet");
                    t.Start();
                    try
                    {
                        ViewSheet sheet;
                        if (tbId.HasValue)
                        {
                            var titleBlock = doc.GetElement(new ElementId(tbId.Value)) as FamilySymbol;
                            if (titleBlock == null) { t.RollBack(); return (object?)null; }
                            if (!titleBlock.IsActive) titleBlock.Activate();
                            sheet = ViewSheet.Create(doc, titleBlock.Id);
                        }
                        else
                        {
                            sheet = ViewSheet.Create(doc, ElementId.InvalidElementId);
                        }

                        if (sheet == null) { t.RollBack(); return (object?)null; }
                        if (!string.IsNullOrWhiteSpace(number)) sheet.SheetNumber = number;
                        if (!string.IsNullOrWhiteSpace(name)) sheet.Name = name;
                        t.Commit();
                        return (object?)(long)sheet.Id.Value;
                    }
                    catch
                    {
                        t.RollBack();
                        return (object?)null;
                    }
                });
            });
    }
}
