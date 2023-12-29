using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Vector {
    public class Feedback {

        public static int MSG = 0, ERROR = 1, SUCCESS = 2, NONE = 3;

        private string msg;
        private int type;

        public Feedback(string msg, int type) {
            this.msg = msg;
            this.type = type >= 0 && type <= 3 ? type : 3;
        }

        public string GetMessage() {
            return msg;
        }

        public int GetCodeType() {
            return type;
        }

        public Color GetColor() {
            switch (type) {
                case 0: return Color.Gray;
                case 1: return Color.Red;
                case 2: return Color.White;
                default: return Color.Orange;
            }
        }

    }

}
