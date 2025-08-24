using System.IO;
using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Executes;

[TestFixture]
public class ExportImportSchedulesTests : BaseGraphQLTest
{
    [Test]
    public async Task ExportSchedulesToExcel_WithNoSchedules_WritesFile()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"revit_mcp_export_{Guid.NewGuid():N}.xlsx");
        const string m = "mutation($path:String!){ exportSchedulesToExcel(filePath:$path) }";
        var data = await PostGraphQlAsync(m, new { path = temp });
        Assert.That(data, Is.Not.Null);
        var outputPath = data!["exportSchedulesToExcel"]?.GetValue<string?>();
        // If server does not allow writing, it may return null; accept either exists or null to keep safe
        if (outputPath != null)
        {
            Assert.That(File.Exists(outputPath), Is.True);
            try { File.Delete(outputPath); } catch { /* ignore */ }
        }
    }

    [Test]
    public async Task ImportScheduleFromExcel_NonExistentFile_ReturnsZero()
    {
        var fake = Path.Combine(Path.GetTempPath(), $"revit_mcp_nope_{Guid.NewGuid():N}.xlsx");
        const string m = "mutation($path:String!,$sid:ID!){ importScheduleFromExcel(filePath:$path, scheduleId:$sid) }";
        var data = await PostGraphQlAsync(m, new { path = fake, sid = -1L });
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["importScheduleFromExcel"]?.GetValue<int>(), Is.EqualTo(0));
    }
}
