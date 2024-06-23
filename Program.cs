// See https://aka.ms/new-console-template for more information

using TarkovDataOrganizer;

Console.WriteLine("Hello, World!");

//await TarkovData.TraderCashOffer.DownloadTable();
//TarkovData.TraderCashOffer.WriteToCsv();
//await TarkovData.TraderBarterOffer.DownloadTable();
//TarkovData.TraderBarterOffer.WriteToCsv();
await TarkovData.TarkovItem.DownloadTable();
TarkovData.TarkovItem.WriteToCsv();

Console.WriteLine("Done!");

