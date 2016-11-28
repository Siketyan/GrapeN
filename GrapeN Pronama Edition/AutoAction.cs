using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrapeN
{
    [Serializable()]
    public class AutoAction
    {
        public ActionType Type { get; set; }
        public string TypeString { get; set; }
        public bool UseRegex { get; set; }
        public string Pattern { get; set; }
        public string Message { get; set; }

        public AutoAction(ActionType Type, bool UseRegex, string Pattern, string Message)
        {
            this.Type = Type;
            this.UseRegex = UseRegex;
            this.Pattern = Pattern;
            this.Message = Message;

            switch (Type)
            {
                case ActionType.Like: TypeString = "いいね"; break;
                case ActionType.Retweet: TypeString = "リツイート"; break;
                case ActionType.Quote: TypeString = "引用リツイート"; break;
                case ActionType.Reply: TypeString = "返信"; break;
            }
        }
    }

    public enum ActionType
    {
        Like,
        Retweet,
        Quote,
        Reply
    }
}
