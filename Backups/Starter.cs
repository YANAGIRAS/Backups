using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO.Compression;
using System.IO;
using System.Linq;
using Implem.Libraries.Utilities;

namespace Backups
{
    class Starter
    {
        static void Main(string[] args)
        {
            Files.Read(@"C:\Users\ophelia\Desktop\Backups\Backups\Database.json").Deserialize<List<Config>>()
                .ForEach(config => backupDatabese(config));
        }


        private static void backupDatabese(Config config)
        {
            var backupFileName = config.backupPath + @"\" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupSql = string.Format(
                @"DBCC SHRINKDATABASE(N'" + config.dbName +
                @"') BACKUP DATABASE [" + config.dbName +
                @"] TO DISK = N'{0}' WITH NOFORMAT, NOINIT, NAME = N'" + config.dbName +
                @"-完全 データベース バックアップ', SKIP, NOREWIND, NOUNLOAD, STATS = 10", backupFileName);
            string connectionString = @"server=tcp:" + config.instanceName +
                @";Database=master;UID=sa;PWD=" + config.saPassword +
                @";Connection Timeout=30;";

            // バックアップ
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            using (SqlCommand slqCmd = new SqlCommand(backupSql, sqlConnection))
            {
                slqCmd.CommandTimeout = 0;
                sqlConnection.Open();
                slqCmd.ExecuteNonQuery();
            }

            // 圧縮とファイル削除
            using (FileStream zipFile = new FileStream(backupFileName + ".zip", FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipFile, ZipArchiveMode.Update))
            {
                archive.CreateEntryFromFile(
                    backupFileName,
                    Path.GetFileNameWithoutExtension(backupFileName));
                File.Delete(backupFileName);
            }
            Console.WriteLine(string.Format("バックアップが完了しました[{0}] ", backupFileName));

            // ZIPファイル「限定で」過去分削除
            // 作成日時ソートなのでファイルシステムトンネリング機能に注意
            var zipfiles = Directory.GetFiles(config.backupPath, @"*.zip")
                .OrderByDescending(file => File.GetCreationTime(file)).ToArray();
            for(int count = zipfiles.Length - 1; config.retensionPeriod.ToInt() <= count; count--)
            {
                File.Delete(zipfiles[count]);
            }
        }
    }
}
