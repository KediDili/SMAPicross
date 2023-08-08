using System.Collections.Generic;

namespace Picrosser
{
    public class PicrossProgress
    {
        public List<string> Completed = new();

        public List<string> GainedQiCoinsFor = new();

        public Dictionary<string, bool?[,]> Continuing = new();

        public bool? NeedsTutorial = null;
    }
}
