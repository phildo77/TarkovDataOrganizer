// See https://aka.ms/new-console-template for more information

using TarkovDataOrganizer;

Console.WriteLine("Hello, World!");

await TarkovData.TraderCashOffer.DownloadTableTraderCashOffers();

TarkovData.TraderCashOffer.WriteToCsv();

Console.WriteLine("Done!");