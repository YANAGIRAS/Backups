using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Implem.Libraries.Utilities;
using Ionic.Zip;
using Ionic.Zlib;

namespace Backups
{
    class Starter
    {
        static void Main(string[] args)
        {
            Files.Read(@"C:\Backups\backupConfig.json").Deserialize<List<Config>>()
                .ForEach(config => backupDatabese(config));
        }


        private static void backupDatabese(Config config)
        {
            var backupFileName = config.backupPath + @"\" + config.dbName + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bak";
            string backupSql = string.Format(
                @"DBCC SHRINKDATABASE(N'" + config.dbName +
                @"') BACKUP DATABASE [" + config.dbName +
                @"] TO DISK = N'{0}' WITH NOFORMAT, NOINIT, NAME = N'" + config.dbName +
                @"-完全 データベース バックアップ', SKIP, NOREWIND, NOUNLOAD, STATS = 10", backupFileName);
            string connectionString = @"server=tcp:" + config.instanceName +
                @";Database=master;UID=sa;PWD=" + config.saPassword +
                @";Connection Timeout=30;";

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            using (SqlCommand slqCmd = new SqlCommand(backupSql, sqlConnection))
            {
                slqCmd.CommandTimeout = 0;
                sqlConnection.Open();
                slqCmd.ExecuteNonQuery();
            }

            using (ZipFile zip = new ZipFile())
            {
                zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                zip.AlternateEncoding = System.Text.Encoding.GetEncoding("shift_jis");
                zip.AlternateEncodingUsage = ZipOption.AsNecessary;
                zip.AddFile(backupFileName, ".");
                zip.Save(config.backupPath + @"\" + Path.GetFileNameWithoutExtension(backupFileName) + ".zip");
            }
            File.Delete(backupFileName);
            Console.WriteLine(string.Format("バックアップが完了しました[{0}] ", backupFileName));


            Directory.GetFiles(config.backupPath, @"*.zip")
                .OrderByDescending(file => File.GetCreationTime(file))
                .Select((o, i) => new { Path = o, Index = i })
                .Where(o => o.Index >= config.retensionPeriod)
                .ForEach(o =>
                    File.Delete(o.Path));
        }
    }
}
