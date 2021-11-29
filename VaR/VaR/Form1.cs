using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VaR.Entities;

namespace VaR
{
    public partial class Form1 : Form
    {
        PortfolioEntities context = new PortfolioEntities();
        List<Tick> ticks;
        List<PortfolioItem> Portfolio = new List<PortfolioItem>();
        List<decimal> Nyereségek;


        public Form1()
        {
            InitializeComponent();

            ticks = context.Ticks.ToList();
            dataGridView1.DataSource = ticks;

            CreatePortflio();


            Nyereségek = new List<decimal>();

            //nem lesz nulla az értéke, mivel itt adunk egy értéket ennek:
            int intervalum = 30;

            DateTime kezdőDátum = (from x in ticks select x.TradingDay).Min();
            DateTime záróDátum = new DateTime(2016, 12, 30);
            TimeSpan z = záróDátum - kezdőDátum;
            for (int i = 0; i < z.Days - intervalum; i++)
            {
                //Az első napon vett érték - a 30. napon vett érték és hozzáadjuk a nyereségekhez
                decimal ny = GetPortfolioValue(kezdőDátum.AddDays(i + intervalum))
                           - GetPortfolioValue(kezdőDátum.AddDays(i));
                Nyereségek.Add(ny);

                //ezt kiírjuk:
                Console.WriteLine(i + " " + ny);
            }

            //Lesz egy lista, mely a nyereségeket mutatja meg az adott napokon és rendezi
            var nyereségekRendezve = (from x in Nyereségek
                                      orderby x
                                      select x)
                                        .ToList();

            //A nyereségek listának az egyik elemét veszi öttel osztva, Ez lesz a VAR érték
            MessageBox.Show(nyereségekRendezve[nyereségekRendezve.Count() / 5].ToString());
        }

        private void CreatePortflio()
        {
            /*Az adatok felvétele történhet így:
             * PortfolioItem p = new PortfolioItem();
            p.Index = "OTP";
            p.Volume = 10;
            Portfolio.Add(p);*/

            //Vagy egy sorba felírhatjük az alábbiak szerint:

            Portfolio.Add(new PortfolioItem() { Index = "OTP", Volume = 10 });
            Portfolio.Add(new PortfolioItem() { Index = "ZWACK", Volume = 10 });
            Portfolio.Add(new PortfolioItem() { Index = "ELMU", Volume = 10 });

            dataGridView2.DataSource = Portfolio;

        }

        private decimal GetPortfolioValue(DateTime date)
        {
            decimal value = 0;
            foreach (var item in Portfolio)
            {
                var last = (from x in ticks
                            where item.Index == x.Index.Trim()
                               && date <= x.TradingDay
                            select x)
                            .First();
                value += (decimal)last.Price * item.Volume;
            }
            return value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter streamWriter = new StreamWriter(saveFileDialog.FileName))
                {

                    int counter = 1;

                    foreach (decimal item in Nyereségek)
                    {
                        streamWriter.WriteLine(string.Format("{0};{1}", counter, item));
                        counter++;
                    }
                }
            }
        }
    }
}
