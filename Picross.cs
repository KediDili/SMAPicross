namespace Picrosser
{
    public class Picross
    {
        public string Name; //name of your picross, required

        public string DrawingPath; //Good, but where's it?

        public string[] MustBeSolvedFirst = System.Array.Empty<string>(); //Any locks?

        internal bool[,] PicrossSolution;

        internal string PackID;
    }
}
