using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CSVToSQL
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                DisplayUsage();
                return;
            }

            bool autoConfirm = args.Contains("-y", StringComparer.OrdinalIgnoreCase);
            var cleanArgs = args.Where(a => !a.Equals("-y", StringComparison.OrdinalIgnoreCase)).ToArray();

            string inputCsvPath = cleanArgs[0];
            string outputSqlPath = cleanArgs[1];
            string tableName = cleanArgs.Length >= 3 ? cleanArgs[2] : Path.GetFileNameWithoutExtension(inputCsvPath);

            bool ignoreFirstLine = false;
            if (cleanArgs.Length >= 4 && bool.TryParse(cleanArgs[3], out bool parsedIgnore))
            {
                ignoreFirstLine = parsedIgnore;
            }

            if (!autoConfirm && !RequestUserConfirmation(tableName))
            {
                return;
            }

            Console.WriteLine($"Démarrage pour la table '{tableName}' | Ignorer 1ère ligne : {(ignoreFirstLine ? "Oui" : "Non")}");

            try
            {
                if (!File.Exists(inputCsvPath))
                {
                    PrintError($"Fichier introuvable : {inputCsvPath}");
                    return;
                }

                string[] lines = File.ReadAllLines(inputCsvPath, Encoding.UTF8);

                if (lines.Length == 0)
                {
                    PrintError("Le fichier CSV est vide.");
                    return;
                }

                char separator = ';'; // Par défaut

                // Si l'utilisateur a passé un 5ème argument (ex: ,) on l'utilise
                if (cleanArgs.Length >= 5)
                {
                    separator = cleanArgs[4][0];
                }
                else
                {
                    // Petite sécurité intelligente : on cherche seulement sur la PREMIÈRE ligne
                    // car souvent les en-têtes n'ont pas de virgules dans le texte.
                    separator = lines[0].Contains(';') ? ';' : ',';
                }

                string regexPattern = Regex.Escape(separator.ToString()) + "(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)";

                ProcessFile(lines, outputSqlPath, tableName, regexPattern, ignoreFirstLine);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Conversion terminée avec succès !");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                PrintError($"Une erreur est survenue : {ex.Message}");
            }
        }

        private static void ProcessFile(string[] lines, string outputSqlPath, string tableName, string regexPattern, bool ignoreFirstLine)
        {
            int startIndex = ignoreFirstLine ? 1 : 0;
            int batchSize = 1000;
            List<string> valuesBatch = new List<string>();
            int totalLinesToProcess = lines.Length - startIndex;
            int processedLines = 0;

            using (StreamWriter writer = new StreamWriter(outputSqlPath, false, Encoding.UTF8))
            {
                for (int i = startIndex; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;

                    string[] values = Regex.Split(lines[i], regexPattern);

                    for (int j = 0; j < values.Length; j++)
                    {
                        string val = values[j].Trim();

                        if (val.StartsWith("\"") && val.EndsWith("\"") && val.Length >= 2)
                        {
                            val = val.Substring(1, val.Length - 2);
                            val = val.Replace("\"\"", "\"");
                        }

                        values[j] = string.IsNullOrEmpty(val) ? "NULL" : "'" + val.Replace("'", "''") + "'";
                    }

                    valuesBatch.Add($"({string.Join(", ", values)})");
                    processedLines++;

                    UpdateProgressBar(processedLines, totalLinesToProcess);

                    if (valuesBatch.Count >= batchSize || i == lines.Length - 1)
                    {
                        string sqlQuery = $"INSERT INTO {tableName} VALUES\n" + string.Join(",\n", valuesBatch) + ";\n";
                        writer.WriteLine(sqlQuery);
                        valuesBatch.Clear();
                    }
                }
            }
        }

        private static bool RequestUserConfirmation(string tableName)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("===============================================================");
            Console.WriteLine(" ⚠️  AVERTISSEMENT IMPORTANT");
            Console.WriteLine("===============================================================");
            Console.WriteLine($" Vous allez générer une insertion pour la table '{tableName}'.");
            Console.WriteLine(" Assurez-vous que votre fichier CSV contient EXACTEMENT le même");
            Console.WriteLine(" nombre de colonnes, dans le même ordre, que votre table SQL.");
            Console.WriteLine("===============================================================");
            Console.ResetColor();

            Console.Write("Voulez-vous continuer ? (O pour Oui, N pour Non) : ");

            ConsoleKeyInfo key = Console.ReadKey(false);
            Console.WriteLine();

            if (key.Key != ConsoleKey.O && key.Key != ConsoleKey.Y)
            {
                PrintError("Opération annulée par l'utilisateur.");
                return false;
            }
            Console.WriteLine();
            return true;
        }

        private static void UpdateProgressBar(int processed, int total)
        {
            double percent = (double)processed / total * 100;
            Console.Write($"\r⏳ Progression : {processed} / {total} lignes ({percent:0.0}%)");
        }

        private static void DisplayUsage()
        {
            Console.WriteLine("❌ Erreur : Arguments manquants.");
            Console.WriteLine("Utilisation : CsvToSql.exe <entree.csv> <sortie.sql> [nom_table] [ignorer_1ere_ligne] [-y]");
            Console.WriteLine("Défaut : ignorer_1ere_ligne = false");
            Console.WriteLine("Exemple : CsvToSql.exe data.csv data.sql MaTable true");
        }

        private static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ {message}");
            Console.ResetColor();
        }
    }
}