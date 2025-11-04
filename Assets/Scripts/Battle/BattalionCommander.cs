using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LongLiveKhioyen
{
    public class BattalionCommander
    {
        public int commanderId;
        public string commanderName;

        public int Yong;
        public int Ren;
        public int Zhi;
        public int Xin;
        public int Yan;

        public BattalionCommander()
        {
            commanderId = 0;
            commanderName = "Nianyi Wang";

            Yong = 50;
            Ren = 50;
            Zhi = 50;
            Xin = 50;
            Yan = 50;
        }
    }
}
