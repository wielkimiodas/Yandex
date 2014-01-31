using System;
using System.Collections.Generic;
using System.IO;
using Yandex.InputFileReader.InputFileReaders;
using Yandex.Utils;
using Yandex.InputFileReader;
using UsersGroup = Yandex.InputFileReader.InputFileReaders.UsersGroup;

namespace Yandex.LogProcessor
{
    public class FinalProcessor
    {
        private readonly StreamReader _reader;

        public FinalProcessor()
        {
            _reader = new StreamReader(PathResolver.ClicksAnalyse);
        }

        public void ProcessTestInput(String inputTestFile, String outputTestFile)
        {
            const int N_GROUPS = Int32.MaxValue;

            MemoryStream ms1 = GetTestFile(inputTestFile);

            while(true)
            {
                ms1.Seek(0, SeekOrigin.Begin);
                List<UsersGroup> groups = ReadGroups(N_GROUPS);
                if (groups.Count == 0)
                    break;

                var ms2 = new MemoryStream();
                var writer = new BinaryWriter(ms2);

                using (var opener = new InputFileOpener(new BinaryReader(ms1), new TestFileReader(writer, groups)))
                {
                    opener.Read();
                }

                ms1.Dispose();
                ms1 = ms2;
            }

            SaveTestFile(ms1, outputTestFile);

            ms1.Dispose();
        }

        private MemoryStream GetTestFile(String inputFile)
        {
            var ms1 = new MemoryStream();
            using (var file = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                var bytes = new byte[file.Length];
                file.Read(bytes, 0, (int)file.Length);
                ms1.Write(bytes, 0, (int)file.Length);
            }

            return ms1;
        }

        private void SaveTestFile(MemoryStream memoryStream, String output)
        {
            using (var file = new FileStream(output, FileMode.CreateNew, FileAccess.Write))
            {
                byte[] bytes = new byte[memoryStream.Length];
                memoryStream.Read(bytes, 0, (int)memoryStream.Length);
                file.Write(bytes, 0, bytes.Length);
                memoryStream.Close();
            }
        }

        private List<UsersGroup> ReadGroups(int n)
        {
            var result = new List<UsersGroup>();
            while (_reader.Peek() > -1)
            {
                result.Add(ReadClickAnalyzerGroup());
                if (result.Count >= n)
                    return result;
            }

            return result;
        }

        private UsersGroup ReadClickAnalyzerGroup()
        {
            var statistics = new List<Tuple<int, float>>();
            var users = new List<int>();
            
            string line;
            while (!string.IsNullOrEmpty(line = _reader.ReadLine()))
            {
                //reading users 
                users.Add(Int32.Parse(line));
            }

            while (!string.IsNullOrEmpty(line = _reader.ReadLine()))
            {
                //reading urls with correlated statistic
                var res = line.Split('\t');
                float val = float.Parse(res[1]);
                statistics.Add(new Tuple<int, float>(Int32.Parse(res[0]), val));
            }

            //returns users and stats
            return new UsersGroup(new BinarySearchSet<int>(users, Comparer<int>.Default), statistics);
        }
    }
}
