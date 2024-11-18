using sec_saver.dependancies;
using System.Xml.Linq;

namespace sec_saver
{
    public class SecSaver
    {
        // structs

        public struct Coord
        {
            public float X; public float Y;
            public Coord(float x, float y)
            {
                X = x;
                Y = y;
            }


            // arithmatic operators
            public static Coord operator +(Coord a, Coord b) => new Coord(a.X + b.X, a.Y + b.Y);
            public static Coord operator +(Coord a, float b) => new Coord(a.X + b, a.Y + b);
            public static Coord operator *(Coord a, Coord b) => new Coord(a.X * b.X, a.Y * b.Y);
            public static Coord operator *(Coord a, float b) => new Coord(a.X * b, a.Y * b);

        }
        private struct Textblock
        {
            public string Text;
            public Coord position;
            public Coord velocity;
        }

        // variables
        private List<Textblock> droppingText;
        private Random rnd;
        private monoprinter window;
        List<string> titles;

        private int width = 50;
        private int height = 50;

        static bool IsPrintable(string Title)
        {
            foreach (char c in Title)
            {
                if (!char.IsAscii(c)) return false;
            }
            return true;
        }


        public async Task Main(string[] args)
        {
            // initialise variables
            droppingText = new List<Textblock>();
            rnd = new Random(100);
            window = new monoprinter((ushort)width, (ushort)height, new Color(0, 0, 0), (byte)0);


            string[] rssUrls = ["https://feeds.feedburner.com/TheHackersNews", "https://api.gdeltproject.org/api/v2/doc/doc?query=cybersecurity&format=rss"];
            titles = new List<string>();

            using (HttpClient client = new HttpClient())
            {
                foreach (string rssUrl in rssUrls)
                {
                    string rssData = await client.GetStringAsync(rssUrl);
                    var xml = XDocument.Parse(rssData);

                    foreach (var item in xml.Descendants("item"))
                    {
                        //Console.WriteLine($"Title: {item.Element("title")?.Value}");
                        //Console.WriteLine($"Link: {item.Element("link")?.Value}");
                        //Console.WriteLine();
                        string title = $"{item.Element("title")?.Value}";
                        if (IsPrintable(title)) { titles.Add(title); }
                    }
                }
            }


            for (int i = 0; i < (width*height) / 100; i++)
            {
                AddDroppingText(titles[rnd.Next(0, titles.Count)]);
            }


            while (true)
            {
                Thread.Sleep(1 / 60 * 1000);
                StepText();
            }






        }



        // rendering logic

        private void AddDroppingText(string text)
        {
            float angle = (float)(rnd.NextDouble() * 2d * Math.PI);
            double power = (float)rnd.Next(400,500) / 3500f;
            float Xvel = (float)(Math.Sin(angle) * power);
            float Yvel = (float)(Math.Cos(angle) * power);

            //float Xvel = (float)(rnd.Next(500, 600)) / 1000.0f; if (rnd.NextDouble() > 0.5) { Xvel *= -1; }
            //float Yvel = (float)(rnd.Next(500, 600)) / 1000.0f; if (rnd.NextDouble() > 0.5) { Yvel *= -1; }

            float Xcoord = 0; float Ycoord = 0;


            if (Math.Abs(Xvel) > Math.Abs(Yvel))
            { // if X velocity is greater thatn Y velocity
                if (Xvel > 0)
                {
                    Xcoord = -text.Length;
                }
                else
                {
                    Xcoord = width;
                }
                Ycoord = rnd.Next(0, height);
            }
            else
            {
                if (Yvel > 0)
                {
                    Ycoord = -text.Length;
                }
                else
                {
                    Ycoord = height;
                }
                Ycoord = rnd.Next(0, height);
            }


            droppingText.Add(new Textblock { Text = text, position = new Coord(Xcoord, Ycoord), velocity = new Coord(Xvel, Yvel) });
        }

        private UInt32[] colourise(string text)
        {
            Stack<char> CharacterStack = new Stack<char>(text.Reverse());
            string buffer = "";
            char peeker = '\0';

            while (CharacterStack.TryPeek(out peeker))
            {

            }
            return uint.MaxValue;
        }

        private void StepText()
        {
            window.fill(new Color(0, 15, 0).packed);
            window.fillCharacter(' ');
            float delta = 1f/60f;
            for (int i = 0; i < droppingText.Count; i++)
            {
                // update position data
                Textblock text = droppingText[i];

                text.position.X = text.position.X + (text.velocity.X * delta);
                text.position.Y = text.position.Y + (text.velocity.Y * delta);
                droppingText [i] = text;

                UInt32[] textColor = new uint[text.Text.Length];


                // draw to screen
                //window.SetPixel((int)text.position.X, (int)text.position.Y, new Color(255, 60, 60).packed);

                for (int c = 0; c < text.Text.Length; c++)
                {
                    UInt32 col = new Color(0, 130, 0).packed;
                    char C = text.Text[c];


                    if (Math.Abs(text.velocity.X) > Math.Abs(text.velocity.Y))
                    {
                        window.SetCharacter(C, col,(int)(text.position.X*2+c), (int)(text.position.Y));
                    }
                    else
                    {
                        window.SetCharacter(C, col, (int)(text.position.X*2), (int)(text.position.Y+c));
                    }
                }


                if (text.position.X < 0 - text.Text.Length || text.position.Y < 0 - text.Text.Length || text.position.X > width || text.position.Y > height)
                {
                    droppingText.Remove(text);
                    AddDroppingText(titles[rnd.Next(0,titles.Count)]);
                }

            }
            // render
            window.Render();
        }
    }
}
