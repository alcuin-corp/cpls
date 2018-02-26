using System.IO;

namespace PLS.Gui
{
    public static class StreamFormattingExtensions {
        public static RowWriter StartTable(this Stream stream) => new RowWriter(stream);
    }
}