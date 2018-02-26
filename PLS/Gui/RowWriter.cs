using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PLS.Gui
{
    public class RowWriter
    {
        private readonly Stream _stream;
        private readonly List<string[]> _rows = new List<string[]>();

        public RowWriter(Stream stream)
        {
            _stream = stream;
        }

        public void AddRow(string[] row) => _rows.Add(row);

        public void AddRows(string[][] rows) => _rows.AddRange(rows);

        public void Write(bool hasHeader = false)
        {
            if (_rows.Any(col => col.Length != _rows[0].Length))
            {
                throw new Exception("Rows have not the same number of columns.");
            }

            var colSizes = new List<int>();

            for (var c = 0; c < _rows[0].Length; c++)
            {
                colSizes.Add(_rows.Select(row => row[c].Length).Max());
            }

            string BuildLine(int length, string character = " ")
            {
                var res = "";
                for (var c = 0; c < length; c++)
                {
                    res += character;
                }
                return res;
            }

            var x = 0;
            const string gap = "    ";
            using (var writer = new StreamWriter(_stream))
            {
                foreach (var row in _rows)
                {
                    var strRow = "";
                    for (var c = 0; c < row.Length; c++)
                    {
                        strRow += row[c] + BuildLine(colSizes[c] - row[c].Length) + gap;
                    }
                    writer.Write(strRow + "\n");
                    if (hasHeader && x == 0) writer.WriteLine(BuildLine(strRow.Length - gap.Length, "-"));
                    x++;
                }
            }
        }
    }
}