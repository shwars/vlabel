using System;
using System.Collections.Generic;
using System.IO;
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

        public Label Clone()
        {
            return new Label()
            {
                Category = this.Category,
                StartFrame = this.StartFrame,
                EndFrame = this.EndFrame
            };
        }

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

        public void MarkHere(int frm, string cat)
        {
            var L = GetLabel(frm, cat);
            if (L==null)
            {
                L = new Label() { Category = cat, StartFrame = frm, EndFrame = frm };
                InsertLabel(cat, L);
            }
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

        public double ScalePos(int frame,double scale = 1.0)
        {
            return ((double)frame) / ((double)VideoFrames) * scale;
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

        public void DeleteLabel(Label L)
        {
            Intervals[L.Category].Remove(L);
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

        public void Save(string fname)
        {
            var s = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            File.WriteAllText(fname, s);
        }

        public static VLabeling Create(string fname)
        {
            var x = new VLabeling() { Filename = fname };
            x.Categories = new string[] { "Default" };
            return x;
        }

        public static VLabeling Load(string fname)
        {
            string s = File.ReadAllText(fname);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<VLabeling>(s);
        }

        public static VLabeling LoadShadow(string fname)
        {
            var fnew = Path.ChangeExtension(fname, Config.ShadowExtension);
            if (!File.Exists(fnew))
            {
                return VLabeling.Create(fname);
            }
            else
            {
                return VLabeling.Load(fnew);
            }
        }

        public string ShadowFilename
        {
            get => Path.ChangeExtension(Filename, Config.ShadowExtension);
        }

        public void SaveShadow()
        {
            Save(ShadowFilename);
        }

        public void Recategorize(int frm, string cat, string new_cat)
        {
            var L = GetLabel(frm, cat);
            if (L!=null)
            {
                DeleteLabel(L);
                L.Category = new_cat;
                InsertLabel(new_cat, L);
            }
        }

        public void Split(int frm, string cat)
        {
            var L = GetLabel(frm, cat);
            if (L == null) return;
            if (frm == L.StartFrame || frm == L.EndFrame) return;
            DeleteLabel(L);
            var M = L.Clone();
            L.EndFrame = frm-1;
            M.StartFrame = frm;
            InsertLabel(cat, L);
            InsertLabel(cat, M);
        }

        public void MergeLeft(int frm, string cat)
        {
            var L = GetLabel(frm, cat);
            if (L == null) return;
            var M = GetLabel(L.StartFrame - 1, cat);
            if (M == null) return;
            DeleteLabel(L);
            M.EndFrame = L.EndFrame;
        }
        public void MergeRight(int frm, string cat)
        {
            var L = GetLabel(frm, cat);
            if (L == null) return;
            var M = GetLabel(L.EndFrame + 1, cat);
            if (M == null) return;
            DeleteLabel(L);
            M.StartFrame = L.StartFrame;
        }

    }
}
