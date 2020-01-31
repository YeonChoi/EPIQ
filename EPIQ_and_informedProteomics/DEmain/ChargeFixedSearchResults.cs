using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InformedProteomics.Backend.Utils;

namespace MMS1Quant
{
    public class ChargeFixedSearchResults: TsvFileParser
    {
        private readonly Dictionary<int, List<int>> _indexByScanNum = new Dictionary<int, List<int>>(); 
 
        public ChargeFixedSearchResults(string fileName, char delimiter = '\t', char titleDelimiter=' ') : base(fileName, delimiter)
        {
            parse_title(titleDelimiter);
        }

        public int GetNumOfSearchResults(int scannum)
        {
            return ScanNumExists(scannum) ? _indexByScanNum[scannum].Count : 0;
        }

        public string GetNstSearchDataOfScanNum(int scannum, int idx, string dataname)
        {
            return _data[dataname][_indexByScanNum[scannum][idx]];
        }

        public Boolean ScanNumExists(int scannum)
        {
            return _indexByScanNum.ContainsKey(scannum);
        }

        public List<int> GetScanNums()
        {
            return new List<int>(_indexByScanNum.Keys);
        } 

        /*
        public List<string> GetDataByScanNum(int scannum, string dataname)
        {
            var retList = new List<string>();
            foreach (int idx in _indexByScanNum[scannum])
            {
                retList.Add(_data[dataname][idx]);

            }
            return retList;
        }
        */

        private void parse_title(char titleDelimiter)
        {
            var titleIndex = -1;
            for (var i = 0; i < _header.Length; i++)
            {
                if (_header[i].Equals("Title")) titleIndex = i;
            }


            var titles = _data[_header[titleIndex]];
            for (var i = 0; i < _rows.Count(); i++)
            {
                try
                {
                    var titleToken = titles[i].Split(titleDelimiter);
                    int realScanNum = Convert.ToInt32(titleToken[0]);
                    if (!_indexByScanNum.ContainsKey(realScanNum))
                    {
                        _indexByScanNum[realScanNum] = new List<int>();
                    }
                    _indexByScanNum[realScanNum].Add(i);
                }
                catch (System.FormatException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(@"{0}", titles[i]);
                    throw new FormatException();
                    break;
                }
            }
        }
    }
}
