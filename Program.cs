using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;

namespace lost_key_whitelist_verification
{

    class SnapshotItem
    {
        public string ethkey { get; set; }
        public string eosaccount { get; set; }
        public string eoskey { get; set; }
        public string balance { get; set; }

        public Decimal getEOSBalance()
        {
            return Convert.ToDecimal(this.balance);
        }

    }

    class WhitelistItem
    {
        public string account { get; set; }
        public string eth_address { get; set; }
    }

    class Program
    {

        static List<SnapshotItem> genesisSnapshotLines = new List<SnapshotItem>();
        static List<WhitelistItem> whitelistLines = new List<WhitelistItem>();

        static void Main(string[] args)
        {
            // snapshot.csv sourceed from https://github.com/eosnewyork/snapshots/tree/master/final/2 (contains 163930 records)
            loadSnaphotCSV("snapshot.csv");
            Console.WriteLine("{0} snapshot recods loaded", genesisSnapshotLines.Count);
            // whitelisted.csv (csv generated using EOSDotNET) https://github.com/eosnewyork/EOSDotNet 
            // The tool essentially steps through the table structure similar to this:
            // cleos -u https://api.eosnewyork.io get table unusedaccnts unusedaccnts whitelist
            // whitelisted.csv contains 95334 records
            loadWhitelistCSV("whitelisted.csv");
            Console.WriteLine("{0} whitelist records loaded", whitelistLines.Count);

            // Loop through the while list and check it against the Genesis snapshot. If anything doesn't match, exist with warning. 
            foreach (var whitelistItem in whitelistLines)
            {
                var genesisRecordMatch = genesisSnapshotLines.Find(i => i.eosaccount == whitelistItem.account);
                if(genesisRecordMatch == null)
                {
                    Console.WriteLine("WARNING: Account {0} was not found in the genesis file", whitelistItem.account);
                    break;
                } else
                {
                    if(genesisRecordMatch.ethkey != "0x" + whitelistItem.eth_address)
                    {
                        Console.WriteLine("WARNING: Account {0} in the white list has eth key {1}, however in the genesis snapshot it's listed as {2}", whitelistItem.account, whitelistItem.eth_address, genesisRecordMatch.getEOSBalance());
                        break;
                    }
                    
                    if(genesisRecordMatch.getEOSBalance() >= 100000)
                    {
                        Console.WriteLine("BALANCE: Account {0} has balance {1} - {2}", whitelistItem.account, genesisRecordMatch.balance, genesisRecordMatch.getEOSBalance().ToString("#.####"));
                    }


                }
                    

            }
            
        }

        static void loadSnaphotCSV(string filePath)
        {
            TextReader reader = new StreamReader(filePath);
            var csvReader = new CsvReader(reader);
            using (var csv = new CsvReader(reader))
            {
                var records = csv.GetRecords<SnapshotItem>();
                foreach (var record in records)
                {
                    genesisSnapshotLines.Add(record);
                }
            }
        }

        static void loadWhitelistCSV(string filePath)
        {
            TextReader reader = new StreamReader(filePath);
            var csvReader = new CsvReader(reader);
            using (var csv = new CsvReader(reader))
            {
                var records = csv.GetRecords<WhitelistItem>();
                foreach (var record in records)
                {
                    whitelistLines.Add(record);
                }
            }
        }

    }
}
