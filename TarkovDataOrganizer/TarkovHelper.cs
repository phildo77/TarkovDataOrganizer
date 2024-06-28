using System.Globalization;
using System.Text;

namespace TarkovDataOrganizer;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

public static class TarkovHelper
{
    public static void WriteToCsv<T>(string _filename, List<T> _dataList, char _delimiter = '~')
    {
        using var writer = new StreamWriter(_filename);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(_dataList);

        Console.WriteLine("Successfully wrote data to '" + _filename + "'");

    }

    public static string WriteCsvToString<T>(List<T> _dataList, char _delimiter = '~')
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(_dataList);

        return sb.ToString();
    }
}