using System;
using System.Collections.Generic;
using System.IO;

namespace Epiq
{
    public class SearchResults
    {
        private readonly double _cutFieldCutOff;
        private readonly string _cutFieldName;
        private readonly char _delimeter;
        private readonly Dictionary<int, List<int>> _indexByScanNum = new Dictionary<int, List<int>>();
        private readonly char _titleDelimiter;
        protected Dictionary<string, List<string>> _data;
        public string[] Header;

        public SearchResults(string fileName, bool chargeFixed = false, double cufFieldCutOff = 1E-7,
            char delimiter = '\t', char titleDelimiter = ' ',
            string cutFieldName = @"SpecEValue")
        {
            FileName = fileName;
            _delimeter = delimiter;
            _titleDelimiter = titleDelimiter;
            _cutFieldName = cutFieldName;
            _cutFieldCutOff = cufFieldCutOff;

            Parse();

            if (chargeFixed)
                index_rows_by_scannum_chargeFixed();
            else
                index_rows_by_scannum();
        }

        public string FileName { get; private set; }

        private void Parse()
        {
            _data = new Dictionary<string, List<string>>();
            var firstRow = true;
            var cutFieldIdx = 0;
            foreach (var line in File.ReadLines(FileName))
            {
                var token = line.Split(_delimeter);
                if (firstRow) // Parse header
                {
                    Header = new string[token.Length];
                    for (var i = 0; i < token.Length; i++)
                    {
                        Header[i] = token[i];
                        _data[Header[i]] = new List<string>();
                        if ((_cutFieldName != null) && (Header[i] == _cutFieldName)) cutFieldIdx = i;
                    }
                    firstRow = false;
                    continue;
                }

                if (token.Length != Header.Length) continue;
                if ((_cutFieldName != null) && (Convert.ToDouble(token[cutFieldIdx]) >= _cutFieldCutOff)) continue;
                for (var i = 0; i < token.Length; i++)
                    _data[Header[i]].Add(token[i]);
            }
        }

        private void index_rows_by_scannum()
        {
            for (var i = 0; i < _data[Header[0]].Count; i++)
            {
                var scanNum = Convert.ToInt32(_data["ScanNum"][i]);
                if (!_indexByScanNum.ContainsKey(scanNum))
                    _indexByScanNum[scanNum] = new List<int>();
                _indexByScanNum[scanNum].Add(i);
            }
        }

        private void index_rows_by_scannum_chargeFixed()
        {
            var titleIndex = -1;
            for (var i = 0; i < Header.Length; i++)
                if (Header[i].Equals("Title")) titleIndex = i;
            if (titleIndex == -1)
                throw new Exception("Title Column is not found. Maybe you are not using a charge fixed search file.");


            var titles = _data[Header[titleIndex]];
            for (var i = 0; i < _data[Header[0]].Count; i++)
                try
                {
                    var titleToken = titles[i].Split(_titleDelimiter);
                    var realScanNum = Convert.ToInt32(titleToken[0]);
                    if (!_indexByScanNum.ContainsKey(realScanNum))
                        _indexByScanNum[realScanNum] = new List<int>();
                    _indexByScanNum[realScanNum].Add(i);
                }
                catch (FormatException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(@"{0}", titles[i]);
                    throw new FormatException();
                    break;
                }
        }

        public int GetNumOfSearchResults(int scannum)
        {
            return ScanNumExists(scannum) ? _indexByScanNum[scannum].Count : 0;
        }

        public string GetNstSearchDataOfScanNum(int scannum, int idx, string dataname)
        {
            return _data[dataname][_indexByScanNum[scannum][idx]];
        }

        public bool ScanNumExists(int scannum)
        {
            return _indexByScanNum.ContainsKey(scannum);
        }

        public List<int> GetScanNums()
        {
            return new List<int>(_indexByScanNum.Keys);
        }

        public List<string> GetDataByScanNum(int scannum, string dataname)
        {
            var retList = new List<string>();
            foreach (var idx in _indexByScanNum[scannum])
                retList.Add(_data[dataname][idx]);
            return retList;
        }

        public string GetDataOfID(Ms2Result ms2Result, string dataname)
        {
            int matchedIdx = -1;
            foreach (var idx in _indexByScanNum[ms2Result.ScanNum])
                if (ms2Result.Peptide == _data[Ms2Result.PeptideHeader][idx])
                    matchedIdx = idx;

            if (matchedIdx==-1)
                throw new Exception("Queried ID not found in original search file. Search file may be currupted?");

            return _data[dataname][matchedIdx];
        }

        public List<string> GetAllDataOfID(Ms2Result ms2Result)
        {
            int matchedIdx = -1;
            foreach (var idx in _indexByScanNum[ms2Result.ScanNum])
                if (ms2Result.Peptide == _data[Ms2Result.PeptideHeader][idx])
                    matchedIdx = idx;

            // add sanity chek here
            if (matchedIdx==-1)
                throw new Exception("Queried ID not found in original search file. Search file may be currupted?");

            var retList = new List<string>();
            foreach (var field in Header)
            {
                retList.Add(_data[field][matchedIdx]);
            }
            return retList;
        }

    }
}