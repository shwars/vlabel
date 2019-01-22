using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vlabel
{
    public class Label
    {
        public string Category { get; set; }
        public int StartFrame { get; set; }

        public int EndFrame { get; set; }
    }

    public class VLabeling
    {
        public string Filename { get; set; }
        public string[] Categories { get; set; }
        public int VideoFrames { get; set; }
        public Dictionary<string, List<Label>> Intervals { get; set; } = new Dictionary<string, List<Label>>();

        public bool GetStatus(int frm, string cat)
        {
            return GetLabel(frm, cat) != null;
        }

        public Label GetLabel(int frm, string cat)
        {
            if (!Intervals.ContainsKey(cat)) return null;
            var l = Intervals[cat];
            return l.FirstOrDefault(I => I.StartFrame <= frm && frm <= I.EndFrame );
        }

        public Label NextLabel(int frm, string cat)
        {
            if (!Intervals.ContainsKey(cat)) return null;
            var l = Intervals[cat];
            return l.OrderBy(I => I.StartFrame).FirstOrDefault(I => I.StartFrame > frm && I.EndFrame > frm);
        }

        public Label PrevLabel(int frm, string cat)
        {
            if (!Intervals.ContainsKey(cat)) return null;
            var l = Intervals[cat];
            return l.OrderBy(I => -I.EndFrame).FirstOrDefault(I => I.StartFrame < frm && I.EndFrame < frm);
        }


        public void MarkIn(int frm, string cat)
        {
            var L = GetLabel(frm, cat);
            if (L != null) L.StartFrame = frm;
            else
            {
                int endf = VideoFrames;
                L = NextLabel(frm, cat);
                if (L != null) endf = L.StartFrame - 1;
                L = new Label() { Category = cat, StartFrame = frm, EndFrame = endf };
                InsertLabel(cat, L);
            }
        }

        public void MarkOut(int frm, string cat)
        {
            var L = GetLabel(frm, cat);
            if (L != null) L.EndFrame = frm;
            else
            {
                int begf = 0;
                L = PrevLabel(frm, cat);
                if (L != null) L.EndFrame = frm;
                else
                {
                    L = new Label() { Category = cat, StartFrame = begf, EndFrame = frm };
                    InsertLabel(cat, L);
                }
            }
        }


        public void InsertLabel(string cat, Label L)
        {
            if (!Intervals.ContainsKey(cat)) Intervals[cat] = new List<Label>();
            Intervals[cat].Add(L);
        }

        public void DeleteLabel(int frm, string cat)
        {
            if (!Intervals.ContainsKey(cat)) return;
            var L = GetLabel(frm, cat);
            if (L!=null) Intervals[cat].Remove(L);
        }

        public int PrevMark(int frm, string cat)
        {
            if (!Intervals.ContainsKey(cat)) return frm;
            var f =
                Intervals[cat].Select(x => x.StartFrame)
                .Union(Intervals[cat].Select(x => x.EndFrame))
                .Where(x => x < frm)
                .OrderBy(x => -x)
                .FirstOrDefault();
            return f;
        }

        public int NextMark(int frm, string cat)
        {
            if (!Intervals.ContainsKey(cat)) return frm;
            var f =
                Intervals[cat].Select(x => x.StartFrame)
                .Union(Intervals[cat].Select(x => x.EndFrame))
                .Where(x => x > frm)
                .OrderBy(x => x)
                .FirstOrDefault();
            return f==0 ? frm : f;
        }


    }
}
