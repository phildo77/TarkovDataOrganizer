// See https://aka.ms/new-console-template for more information

using TarkovDataOrganizer;

Console.WriteLine("Tarkov.dev data organizer - Downloading Data into CSV files...");

await TarkovData.CashOffer.DownloadTable();
//TarkovData.TraderCashOffer.WriteToCsv();
await TarkovData.BarterOffer.DownloadTable();
//TarkovData.TraderBarterOffer.WriteToCsv();
await TarkovData.TarkovItem.DownloadTable();
TarkovData.TarkovItem.WriteToCsv();

//Testing Colt assault rifle
//TarkovData.GunModCombinator.GunConfig.Build("5447a9cd4bdc2dbd208b4567");
TarkovData.GunModCombinator.GunConfig.TestWriteAllCombosToFile("5447a9cd4bdc2dbd208b4567");


Console.WriteLine("Done!");

