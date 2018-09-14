using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Diagnostics;
using System.IO;

namespace toroid
{
    public partial class Form1 : Form
    {
        static public int brz_prim, brz_sek, ssek1;
        static public string RBR;
        static public double f, fak_3, snaga, r_snaga, Afe, Dizo = 0.2;
        static public double un_pr_je, va_pr_je, sir_je, T_jez, prim_U, sek_U, i_prim, i_sek;
        static public double sir_izo, dulj_izo, dulj_izo_pr;
        static public double n_prim, n_sek, dsek, dostn, aostn;
        static public double fi_prim_staro, fi_sek_staro;
        static public double dt, d_nam, gpros, k;
        static public double fi_prim, fi_sek, pr_prim, pr_sek;
        static public double K_U, p_gub_jez, B, pr_sek_u, gsek, Gi_sek;
        static public double lprim, t_prim, rprim, gprim, Gi_prim, dprim, sprim, p_gub, dost_ko;
        static public double s_izo_p, s_izo_s, n_sek_n, n_prim_n, lsek, rsek, t_sek, l_izo_s, suk_ko, vuk_ko;
        static public double ssek, opseg1, opseg2, dost, aost, ajez, acu_sek, acu_prim;
        static public double RW2_PR, RW2_SE, RW3_PR, RW3_SE;

        static public List<string> sifra = new List<string>();      //
        static public List<string> promjer = new List<string>();    //liste za spremanje podataka iz baze (.csv file)
        static public List<string> presjek = new List<string>();    //

        public Form1()
        {
            InitializeComponent();
            citanje_baze_zica();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var timer = new Timer { Interval = 1000 };
            var culture = new System.Globalization.CultureInfo("hr-HR");
            timer.Tick += (o, args) =>  //timer za azuriranje vremena na maski
            {
                lblDate.Text = FirstCharToUpper(culture.DateTimeFormat.GetDayName(DateTime.Today.DayOfWeek).ToString()) + ", " + DateTime.Now.ToString();
            };
            timer.Start();
        }

        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("String is null or empty.");
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        private void listZice1_SelectedIndexChanged(object sender, EventArgs e)
        {
            fi_prim = Convert.ToDouble(listZice1.SelectedValue.ToString().Substring(7, 4));
            brz_prim = Convert.ToInt16(listZice1.SelectedValue.ToString().Substring(0, 2).Trim(' '));
            lblBrojZicaPrimara.Text = "Broj žica primara: " + brz_prim.ToString();
        }

        private void listZice2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string str = listZice2.SelectedValue.ToString().Substring(7, 4);
            if (str == "----") fi_sek = 0;
            else fi_sek = Convert.ToDouble(str);
            brz_sek = Convert.ToInt16(listZice2.SelectedValue.ToString().Substring(0, 2).Trim(' '));
            lblBrojZicaSekundara.Text = "Broj žica sekundara: " + brz_sek.ToString();
        }

        private double izolacija(double sirina_i, double visina, double vani, double unutra, int slojeva)
        {
            //izracun duljine izolacije prema podacima sirini, visini, vanjskom i unutarnjem promjeru i slojevima
            double L;
            double nam_izo = ((vani * Math.PI) / sirina_i) * slojeva;
            double opseg_i = (vani - unutra) + (2 * visina);
            L = Math.Round((nam_izo * opseg_i) / 1000, 2);
            return L;

            /*int slojeva = Convert.ToInt32(txtBrojSlojevaJezgra.Text);
            double nam_izo = ((Form1.va_pr_je * Math.PI) / sir_izo) * slojeva;
            double opseg_i = (Form1.va_pr_je - Form1.un_pr_je) + 2 * sir_je;
            dulj_izo = Math.Round((nam_izo * opseg_i) / 1000, 2); */
        }

        private void btnRacunaj_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtVanjskiPromjer.Text) || string.IsNullOrWhiteSpace(txtUnutarnjiPromjer.Text) || string.IsNullOrWhiteSpace(txtSirinaJezgre.Text))
            {
                MessageBox.Show("Vanjski promjer, unutarnji promjer i sirina jezgre su obavezna polja.");
                return;
            }
            va_pr_je = Convert.ToDouble(txtVanjskiPromjer.Text);                                                                                    //vanjski promjer
            un_pr_je = Convert.ToDouble(txtUnutarnjiPromjer.Text);                                                                                  //untarnji promjer
            sir_je = Convert.ToDouble(txtSirinaJezgre.Text);                                                                                        //sirina jezgre
            Afe = (((va_pr_je - un_pr_je) / 2) * sir_je) / 100;                                                                                     //povrsina presjeka
            txtPovrsinaPresjeka.Text = Afe.ToString("0.00");
            T_jez = (((((Math.Pow(va_pr_je, 2) * Math.PI) / 4) * sir_je) - (((Math.Pow(un_pr_je, 2) * Math.PI) / 4) * sir_je)) / 1000000) * 7.36;   //tezina jezgre
            if (T_jez < 0.99999)
            {
                lblTezina.Text = Math.Round(T_jez * 1000, 0).ToString("0.00") + " (gr) Fe"; //grami ili kilogrami
            }
            else
            {
                lblTezina.Text = Math.Round(T_jez, 3).ToString() + " (kg) Fe";              //grami ili kilogrami
            }
            lblTezina.Show();

            double fak_je = Convert.ToDouble(txtJezgraDH.Text);
            double fak_2 = Convert.ToDouble(txtJezgraFi.Text);
            fak_3 = Convert.ToDouble(txtJezgraFfe.Text);
            f = Convert.ToDouble(txtFrekvencija.Text);  //frekvencija

            snaga = Math.Pow((Afe * 100) / fak_3, 2) * f;
            if (snaga > 1000)
            {
                r_snaga = Math.Round(snaga / 100, 0) * 100; //r_ = rounded; zaokruzena snaga
            }
            else if (snaga > 100)
            {
                r_snaga = Math.Round(snaga / 10, 0) * 10;
            }
            else if (snaga > 10)
            {
                r_snaga = Math.Round(snaga, 0);
            }
            else
            {
                r_snaga = Math.Round(snaga, 1);
            }
            if (r_snaga != snaga)
            {
                lblSnaga.Text = r_snaga.ToString("0.00") + " (VA)";
                lblSnaga.Show();
            }
            txtNazivnaSnaga.Text = Math.Round(snaga, 1).ToString("0.00");
        }

        private void btnRacunajNamotaje_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSekundarniNapon.Text))
            {
                MessageBox.Show("Sekundarni napon je obavezno polje.");
                return;
            }

            f = Convert.ToDouble(txtFrekvencija.Text);
            if (f < 48 || f > 65)
            {
                MessageBox.Show("Neispravna frekvencija. Frekvencija mora biti izmedu 48 i 65.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtNazivnaSnaga.Text))
            {
                snaga = Math.Round(Math.Pow((Afe * 100) / fak_3, 2) * f, 1);
                txtNazivnaSnaga.Text = snaga.ToString();
            }
            else
            {
                snaga = Convert.ToDouble(txtNazivnaSnaga.Text);
            }
            if (snaga > 1000)
            {
                r_snaga = Math.Round(snaga / 100 + 0.499, 0) * 100;
            }
            else if (snaga > 100)
            {
                r_snaga = Math.Round(snaga / 10 + 0.499, 0) * 10;
            }
            else if (snaga > 10)
            {
                r_snaga = Math.Round(snaga + 0.499, 0);
            }
            else
            {
                r_snaga = Math.Round(snaga + 0.0499, 1);
            }
            if (r_snaga != snaga)
            {
                lblSnaga.Text = r_snaga.ToString("0.00") + " (VA)";
                lblSnaga.Show();
            }
            prim_U = Convert.ToDouble(txtPrimarniNapon.Text); //primarni napon
            sek_U = Convert.ToDouble(txtSekundarniNapon.Text); //sekundarni napon
            double B;

            if (prim_U >= 110 && prim_U <= 120) B = (1.54 / 110) * prim_U; //B = indukcija
            else if (prim_U >= 220 && prim_U <= 230) B = (1.54 / 220) * prim_U;
            else if (prim_U >= 380 && prim_U <= 415) B = (1.54 / 380) * prim_U;
            else B = 1.610;

            txtIndukcija.Text = Math.Round(B, 3).ToString("0.000");

            Gi_sek = Math.Round(6.9614507578 / Math.Log10(r_snaga + 25.00), 2);
            Gi_prim = Gi_sek;

            lblGustocaStruje.Text = Gi_prim.ToString() + "/" + Gi_sek.ToString() + " (A/mm²)";
            lblGustocaStruje.Show();

            double nv = 2250 / (f * Afe * B);
            n_prim = Math.Round(prim_U * nv + 0.4999, 0);    //broj namotaja primara
            n_sek = sek_U * nv;                              //broj namotaja sekundara

            lblBrojZavojaPrim.Text = n_prim.ToString();
            lblBrojZavojaSek.Text = Math.Round(n_sek + 0.49999, 0).ToString();
            lblBrojZavojaPrim.Show();
            lblBrojZavojaSek.Show();

            i_prim = (snaga / prim_U) * (1 + ((10.452 / Math.Log10(snaga / 3.23)) / 100));   //struja primara
            i_sek = snaga / sek_U;                                                           //struja sekundara
            fi_prim = Math.Round(1.13 * Math.Sqrt(i_prim / Gi_prim), 3);                     //promjer zice primara
            pr_prim = Math.Round(Math.Pow(fi_prim / 2, 2) * Math.PI, 3);                     //presjek zice primara
            fi_sek = Math.Round(1.13 * Math.Sqrt(i_sek / Gi_sek), 3);                        //promjer zice sekundara
            pr_sek = Math.Round(Math.Pow(fi_sek / 2, 2) * Math.PI, 3);                       //presjek zice sekundara

            fi_prim_staro = fi_prim;
            fi_sek_staro = fi_sek;

            lblPromjerZicePrim.Text = fi_prim.ToString() + "(mm) => " + pr_prim.ToString() + "(mm²)";
            lblPromjerZiceSek.Text = fi_sek.ToString() + "(mm) => " + pr_sek.ToString() + "(mm²)";
            lblPromjerZicePrim.Show();
            lblPromjerZiceSek.Show();

            listZice1.DataSource = izbor_z(fi_prim);
            listZice2.DataSource = izbor_z(fi_sek);
        }

        private void citanje_baze_zica()
        {
            //citanje podataka iz .csv datoteke i spremanje u liste sifri, promjera i presjeka
            using (var reader = new StreamReader(@Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ZICE.csv")))
            {

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    sifra.Add(values[0].Trim('"'));
                    promjer.Add(values[1]);
                    presjek.Add(values[2]);
                }
            }
            sifra.RemoveAt(0);
            promjer.RemoveAt(0);
            presjek.RemoveAt(0);
        }

        private List<string> izbor_z(double fi_zice)
        {
            List<string> returnvalues = new List<string>();
            int index = 0;
            for (int n = 1; n <= 20; n++)
            {
                for (double m = 0.0; m <= 1.0; m += 0.2)
                {
                    string find = (Math.Round((fi_zice / Math.Sqrt(n)) / (1 + m / 100), 3) * 1000).ToString().PadLeft(4, '0') + "/ /";
                    foreach (string sif in sifra)
                    {
                        int sif1 = Convert.ToInt16(find.Substring(0, 4));
                        int sif2 = Convert.ToInt16(sif.Substring(0, 4));
                        if (sif1 - sif2 <= 0) //odabir prve zice veceg promjera
                        {
                            index = sifra.IndexOf(sif);
                            if (Convert.ToDouble(promjer[index]) <= Math.Round(fi_zice / Math.Sqrt(n) * 1.01, 3))
                            {
                                m = 1.1;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                }
                double posto = Math.Round(((Convert.ToDouble(promjer[index]) * Math.Sqrt(n)) / fi_zice - 1) * 100, 2);
                if (Math.Abs(posto) < 5)
                {

                    if (posto > 0) returnvalues.Add(n.ToString().PadLeft(2, ' ') + " x Ø " + promjer[index] + " (+" + posto.ToString("0.00") + "%)");
                    else returnvalues.Add(n.ToString().PadLeft(2, ' ') + " x Ø " + promjer[index] + " (" + posto.ToString("0.00") + "%)");
                }
                else
                {
                    //u originalnom programu, pojedine zice se oznacuju kao "dobra" pomocu nekvog uvjeta i prema tome se ispisuju ili ne ispisuju podaci o njoj,
                    //odnosno prema tome se ona moze ili ne moze odabrati
                    //tu je uvjet drugaciji, ovisi samo o postotku jer ono nije radilo
                    returnvalues.Add(n.ToString().PadLeft(2, ' ') + " x Ø --------------");
                }

            }

            return returnvalues;
        }

        private void btnRacunajDimenzije_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBrojSlojevaJezgra.Text) || string.IsNullOrWhiteSpace(txtBrojSlojevaPrimar.Text) || string.IsNullOrWhiteSpace(txtBrojSlojevaSekundar.Text))
            {
                MessageBox.Show("Broj slojeva je obavezno polje!");
                return;
            }
            if (listZiceJezgra.SelectedItem == null || listZicePrimar.SelectedItem == null || listZiceSekundar.SelectedItem == null)
            {
                MessageBox.Show("Odaberite žicu.");
                return;
            }
            //i_prim = snaga / prim_U;


            double un_pr_no = un_pr_je - 6 * Dizo;
            double va_pr_no = va_pr_je + 6 * Dizo;
            double sir_no = sir_je + 6 * Dizo;

            T_jez = ((((((Math.Pow(va_pr_je, 2) * Math.PI) / 4) * sir_je) - (((Math.Pow(un_pr_je, 2) * Math.PI) / 4) * sir_je)) / 1000000) * 7.395);

            if (T_jez < 0.1)
                lblPovrsinaPresjekaITezina.Text = "Površina presjeka: " + Afe.ToString("0.00") + "(cm²) => " + Math.Round(T_jez * 1000, 0).ToString() + "(gr) Fe";
            else
                lblPovrsinaPresjekaITezina.Text = "Površina presjeka: " + Afe.ToString("0.00") + "(cm²) => " + Math.Round(T_jez, 2).ToString("0.000") + "(kg) Fe";
            lblPovrsinaPresjekaITezina.Visible = true;

            btnIspis.Enabled = true;

            sir_izo = Convert.ToDouble(listZiceJezgra.SelectedItem.ToString().Substring(0, 4).Replace(".", ","));

            int slojeva = Convert.ToInt32(txtBrojSlojevaJezgra.Text);
            /* 
            double nam_izo = ((Form1.va_pr_je * Math.PI) / sir_izo) * slojeva;
            double opseg_i = (Form1.va_pr_je - Form1.un_pr_je) + 2 * sir_je;
            dulj_izo = Math.Round((nam_izo * opseg_i) / 1000, 2);
            */

            p_gub_jez = T_jez * 2.50;

            dulj_izo = izolacija(sir_izo, sir_je, va_pr_je, un_pr_je, slojeva);

            acu_prim = n_prim * Math.Pow(fi_prim * 1.05, 2) * brz_prim;
            ajez = (Math.Pow(un_pr_no, 2) * Math.PI) / 4;
            aost = ajez - acu_prim;
            dost = Math.Sqrt((aost * 4) / Math.PI);
            dprim = (un_pr_no - dost) / 2;
            dost = un_pr_no - (dprim * 2);
            sprim = Math.Round((n_prim * brz_prim) / (((un_pr_no * Math.PI + dost * Math.PI) / 2) / (fi_prim + 0.05)) + 0.499999, 0);
            dost = un_pr_no - (sprim * (fi_prim + 0.05) * 2);
            dprim = (fi_prim * 1.05) * sprim;
            aost = Math.Pow(dost, 2) * Math.PI / 4;
            double suk = Math.Sqrt(Math.Pow(va_pr_no, 2) + Math.Pow(un_pr_no, 2) - Math.Pow(dost, 2));
            double lcu_pr = (2 * sir_no) + ((Math.PI * (un_pr_no - dost + suk - va_pr_no)) / 4) + Math.Sqrt(Math.Pow(va_pr_no - un_pr_no, 2) + (Math.Pow(un_pr_no - dost + suk - va_pr_no, 2) / 4));
            lcu_pr = lcu_pr * 1.0;
            lprim = Math.Round((lcu_pr * n_prim * brz_prim) / 1000, 1);
            rprim = 0.022 * ((lprim / brz_prim) / (Math.Pow(fi_prim, 2) * brz_prim));
            t_prim = 7 * Math.Pow(fi_prim, 2) * lprim;
            dost = dost - (Dizo * 6);
            double vuk = sir_no + (dprim * 2) + (Dizo * 6);
            double s_dprim = (suk - va_pr_no + un_pr_no - dost) / 2;
            double un_pr_pr = un_pr_no - (2 * s_dprim);
            double va_pr_pr = va_pr_no + (2 * s_dprim);
            double sir_pr = sir_no + (2 * s_dprim);
            t_prim = Math.Round(t_prim, 2);

            //-------------------- izolacija za primar ---------------

            acu_sek = n_sek * Math.Pow(fi_sek * 1.05, 2) * brz_sek;

            aost = ajez - acu_prim;
            aostn = aost - acu_sek;
            dostn = Math.Sqrt((aostn * 4) / Math.PI);
            dsek = (dost - dostn) / 2;
            opseg1 = dost * Math.PI;
            opseg2 = dostn * Math.PI;
            ssek = Math.Round((n_sek * brz_sek) / (((opseg1 + opseg2) / 2) / (fi_sek * 1.05)) + 0.499999, 0);
            dsek = fi_sek * ssek * 1.05;
            dost = dost - dsek * 2 - Dizo * 6;

            vuk = sir_no + dprim * 2 + dsek * 2 + Dizo * 18;
            suk = Math.Sqrt(Math.Pow(va_pr_no, 2) + Math.Pow(un_pr_no, 2) - Math.Pow(dost, 2));
            double lcu_se = (2 * sir_pr) + ((Math.PI * (un_pr_pr - dost + suk - va_pr_pr)) / 4) + Math.Sqrt(Math.Pow(va_pr_pr - un_pr_pr, 2) + (Math.Pow(un_pr_pr - dost + suk - va_pr_pr, 2) / 4));
            lcu_se = lcu_se * 1.000;
            lsek = Math.Round((lcu_se * n_sek * brz_sek) / 1000, 1);
            rsek = 0.022 * ((lsek / brz_sek) / (Math.Pow(fi_sek, 2) * brz_sek));

            //--------

            double pad_u_pr = rprim * (i_prim + rsek * Math.Pow(i_sek, 2) / prim_U + p_gub_jez / prim_U);
            double pad_u_se = pad_u_pr * sek_U / (prim_U - pad_u_pr) + rsek * i_sek;
            K_U = 1 / (1 - (pad_u_se / sek_U));
            ssek = Math.Round((n_sek * K_U * brz_sek) / (((opseg1 + opseg2) / 2) / (fi_sek * 1.05)) + 0.499999, 0);
            ssek1 = Convert.ToInt32(Math.Round(Math.Round((n_sek * K_U) + 0.49999, 0) / (opseg2 / (fi_sek * 1.05)) + 0.499999, 0));
            dsek = fi_sek * ssek * 1.05;

            n_sek_n = Math.Round((n_sek * K_U) + 0.499999, 0);
            n_prim_n = Math.Round((n_sek_n / K_U) * (prim_U / sek_U), 0); // u originalnom kodu je n_sek / K_U  ... ali onda nije tocno

            B = Math.Round(2250 / (f * Afe * (n_prim_n / prim_U)), 3);
            pr_sek_u = Math.Round((prim_U / n_prim_n) * n_sek_n, 2);
            if (pr_sek_u >= 20.00)
            {
                pr_sek_u = Math.Round(pr_sek_u, 1);
            }

            //---------------- say ....
            t_sek = 7 * Math.Pow(fi_sek, 2) * lsek;
            dost_ko = un_pr_je - (Dizo * 18) - (dprim * 2) - (dsek * 2);
            vuk_ko = sir_je + (Dizo * 18) + (dprim * 2) + (dsek * 2);
            suk_ko = suk + fi_sek;
            t_sek = Math.Round(t_sek * K_U, 2);
            //-- say t_sek

            rsek = rsek * K_U;
            //-- say rsek

            lsek = lsek * K_U;

            //--- izolacija sekundara

            //sprim = Math.Round((n_prim * brz_prim) / (((un_pr_no * Math.PI + dost * Math.PI) / 2) / (fi_prim + 0.05)) + 0.499999, 0);



            lblJezgra.Text = "JEZGRA: " + Math.Round(snaga, 0).ToString() + " / " + Math.Round(snaga, 1).ToString("0.00") + " (VA)   B = " + B.ToString("0.000") + "(T)";
            lblPrimar.Text = "PRIMAR: " + prim_U.ToString("0.00") + " (V) / " + Math.Round(i_prim, 3).ToString("0.000") + " (A)";

            i_prim = snaga / prim_U;

            gprim = (i_prim * K_U + p_gub_jez / prim_U) / (Math.Pow(fi_prim / 2, 2) * Math.PI * brz_prim);
            s_izo_p = Convert.ToDouble(listZicePrimar.SelectedItem.ToString().Substring(0, 4).Replace(".", ","));
            s_izo_s = Convert.ToDouble(listZiceSekundar.SelectedItem.ToString().Substring(0, 4).Replace(".", ","));

            int slojevi_primar = Convert.ToInt32(txtBrojSlojevaPrimar.Text);
            dulj_izo_pr = izolacija(s_izo_p, sir_pr, va_pr_pr, un_pr_pr, slojevi_primar);



            gsek = i_sek / (Math.Pow(fi_sek / 2, 2) * Math.PI * brz_sek);

            int slojevi_sekundar = Convert.ToInt32(txtBrojSlojevaSekundar.Text);
            l_izo_s = izolacija(s_izo_s, vuk_ko, suk, dost_ko, slojevi_sekundar);

            p_gub = Math.Pow(i_sek, 2) * rsek + Math.Pow((i_prim * K_U + p_gub_jez / prim_U), 2) * rprim;

            RW2_PR = Math.Round(lprim / 0.63 + 0.499999, 0);
            RW3_PR = Math.Round(lprim / 0.64 + 0.499999, 0);
            RW2_SE = Math.Round(lsek / 0.63 + 0.499999, 0);
            RW3_SE = Math.Round(lsek / 0.64 + 0.499999, 0);

            lblSekundar.Text = "SEKUNDAR: " + sek_U.ToString("0.00") + "(V) / " + Math.Round(i_sek, 2).ToString() + " (A)";

            lblPromjerZicePrimNovo.Text = "=> " + fi_prim.ToString("0.000") + "(mm)";
            lblPromjerZicePrimNovo.Visible = true;
            lblPromjerZiceSekNovo.Text = "=> " + fi_sek.ToString("0.000") + "(mm)";
            lblPromjerZiceSekNovo.Visible = true;

            if (fi_prim == 0) k = 0.47;
            else if (fi_prim <= 0.15) k = 0.61;
            else if (fi_prim <= 0.25) k = 0.51;
            else if (fi_prim <= 0.4) k = 0.51;
            else if (fi_prim <= 1.5) k = 0.46;
            else k = 0.43;

            lblZagrijavanje.Visible = true;
            lblZagrijavanje.Text = "Gp = " + gprim.ToString("0.00") + ", Gs = " + gsek.ToString("0.00") + " Ø = " + fi_prim.ToString("0.00") + " => K= " + k.ToString("0.00");

            gpros = (gprim + gsek) / 2;
            d_nam = dprim + dsek;

            txtDebljinaNamotaja.Text = d_nam.ToString("0.0");
            txtMaxGustocaStruje.Text = gpros.ToString("0.00");

            RBR = txtRBR.Text;
        }

        private void btnIspis_Click(object sender, EventArgs e)
        {
            //var forma = new formIspis();
            //forma.Show();
            int poz = 30;
            int lpoz = 20;
            var culture = new System.Globalization.CultureInfo("hr-HR");
            PdfDocument pdf = new PdfDocument();
            XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
            pdf.Info.Title = "Toroid ispis 1";
            PdfPage pdfPage = pdf.AddPage();
            XGraphics graph = XGraphics.FromPdfPage(pdfPage);
            XFont font = new XFont("Consolas", 12, XFontStyle.Regular, options);
            graph.DrawString(FirstCharToUpper(culture.DateTimeFormat.GetDayName(DateTime.Today.DayOfWeek).ToString()) + ", " + DateTime.Now.ToString(), font, XBrushes.Black, new XRect(lpoz, 10, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("-------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("TOROID d.o.o.            PRORAČUN TOROIDNOG TRANSFORMATORA                Broj: " + RBR, font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("-------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("JEZGRA: " + Math.Round(snaga, 0) + " / " + Math.Round(snaga, 1).ToString("0.00") + " (VA)   B = " + B.ToString("0.000") + " (T)", font, XBrushes.Black, new XRect(50, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("-------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Vanjski promjer: " + va_pr_je.ToString("0.00") + " (mm)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);//
            graph.DrawString("Unutarnji promjer: " + un_pr_je.ToString("0.00") + " (mm)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Širina jezgre: " + sir_je.ToString("0.00") + " (mm)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Površina presjeka: " + Afe.ToString("0.00") + " (mm²)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            string tezina = T_jez < 0.1 ? Math.Round(T_jez * 1000, 0).ToString() + " (gr)" : Math.Round(T_jez, 2).ToString("0.000") + " (kg)";
            graph.DrawString("Težina jezgre: " + tezina, font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Širina izolacije: " + sir_izo.ToString() + " (mm)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Duljina izolacije: " + dulj_izo.ToString() + " (m)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("-------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("PRIMAR: " + prim_U.ToString("0.00") + " (V) / " + Math.Round(i_prim * K_U + p_gub_jez / prim_U, 3).ToString("0.000") + " (A)", font, XBrushes.Black, new XRect(50, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("-------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            if (brz_prim == 1) graph.DrawString("Broj zavoja: " + n_prim_n.ToString(), font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            else graph.DrawString("Broj zavoja: " + n_prim.ToString() + " x " + brz_prim.ToString() + " žica = " + (n_prim * brz_prim).ToString(), font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Promjer žice: " + fi_prim.ToString("0.000") + " (" + (Math.Round(fi_prim_staro / Math.Sqrt(brz_prim), 3)).ToString("0.000") + ") (mm)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Broj žica: " + brz_prim.ToString(), font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Duljina žice: " + lprim.ToString("0.00") + " (m)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            if (t_prim < 1000) graph.DrawString("Težina žice: " + t_prim.ToString("0.00") + " (gr)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            else graph.DrawString("Težina žice: " + Math.Round(t_prim / 1000, 3).ToString("0.000") + " (kg)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            if (rprim >= 1) graph.DrawString("Omski otpor primara: " + Math.Round(rprim, 3).ToString("0.000") + " (Ω)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            else graph.DrawString("Omski otpor primara: " + Math.Round(rprim * 1000, 3).ToString("0.000") + " (mΩ)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Gustoća struje: " + Math.Round(gprim, 2).ToString("0.00") + " (" + Math.Round(Gi_prim, 2).ToString() + ") (A/mm²)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Debljina primara: " + Math.Round(dprim, 1).ToString("0.0") + " (mm) => " + sprim.ToString() + " slojeva", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Širina izolacije: " + s_izo_p.ToString() + " (mm)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Duljina izolacije: " + dulj_izo_pr.ToString() + " (m)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("-------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("SEKUNDAR: " + sek_U.ToString("0.00") + " (V) / " + Math.Round(i_sek, 2).ToString("0.00") + " (A)", font, XBrushes.Black, new XRect(50, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("-------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Napon praz. hoda: " + pr_sek_u.ToString("0.00") + " (V) => K = " + Math.Round((K_U - 1) * 100, 2) + " (%)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            if (brz_sek == 1) graph.DrawString("Broj zavoja: " + n_sek_n.ToString(), font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            else graph.DrawString("Broj zavoja: " + n_sek_n.ToString() + " x " + brz_sek.ToString() + " žica = " + (brz_sek * n_sek_n).ToString(), font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Promjer žice: " + fi_sek.ToString("0.000") + " (" + (Math.Round(fi_sek_staro / Math.Sqrt(brz_sek), 3)).ToString("0.000") + ") (mm)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Broj žica: " + brz_sek.ToString(), font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Duljina žice: " + lsek.ToString("0.00") + " (m)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            if (t_sek < 1000) graph.DrawString("Težina žice: " + t_sek.ToString("0.0") + " (gr)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            else graph.DrawString("Težina žice: " + Math.Round(t_sek / 1000, 3).ToString("0.000") + " (kg)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            if (rsek >= 1) graph.DrawString("Omski otpor sekundara: " + Math.Round(rsek, 3).ToString("0.0000") + " (Ω)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            else graph.DrawString("Omski otpor sekundara: " + Math.Round(rsek * 1000, 3).ToString("0.000") + " (mΩ)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Gustoća struje: " + gsek.ToString("0.00") + " (" + Math.Round(Gi_sek, 2).ToString("0.00") + ") (A/mm²)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Debljina sekundara: " + Math.Round(dsek, 1).ToString("0.0") + " (mm) => " + ssek1.ToString() + " slojeva", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Širina izolacije: " + s_izo_s.ToString() + " (mm)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Duljina izolacije: " + l_izo_s.ToString("0.00") + " (m)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("-------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("DIMENZIJE NAMOTANOG TRANSFORMATORA", font, XBrushes.Black, new XRect(50, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("-------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Vanjski promjer: " + Math.Round(suk_ko, 1).ToString("0.0") + " (mm)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Untarnji promjer: " + Math.Round(dost_ko, 1).ToString("0.0") + " (mm)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Ukupna visina: " + Math.Round(vuk_ko, 1).ToString("0.0") + " (mm)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Težina transformatora: " + Math.Round((T_jez * 1000) + t_prim + t_sek, 1).ToString("0.0") + " (gr)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("Termički gubici: " + Math.Round(p_gub, 1).ToString("0.0") + " (W)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
            graph.DrawString("---------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);

            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string pdfFilename = System.IO.Path.Combine(desktop, "toroid_ispis_1.pdf");
            pdf.Save(pdfFilename);
            Process.Start(pdfFilename);
            // -----------------------------------------------------------------------------------------------------------------------
            // -----------------------------------------------------------------------------------------------------------------------
            // -----------------------------------------------------------------------------------------------------------------------
            PdfDocument pdf2 = new PdfDocument();
            pdf2.Info.Title = "Toroid ispis 2";
            PdfPage pdfPage2 = pdf2.AddPage();
            XGraphics graph2 = XGraphics.FromPdfPage(pdfPage2);
            poz = 30;
            graph2.DrawString(FirstCharToUpper(culture.DateTimeFormat.GetDayName(DateTime.Today.DayOfWeek).ToString()) + ", " + DateTime.Now.ToString(), font, XBrushes.Black, new XRect(lpoz, 10, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("-------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("TOROID d.o.o.           NALOG ZA IZRADU TOROIDNIH TRANSFORMATORA          Broj: " + RBR, font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("-------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("JEZGRA: " + snaga.ToString("0.00") + " (VA)", font, XBrushes.Black, new XRect(50, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("-------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("Vanjski promjer: " + va_pr_je.ToString("0.00") + " (mm)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("Unutarnji promjer: " + un_pr_je.ToString("0.00") + " (mm)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("Širina jezgre: " + sir_je.ToString("0.00") + " (mm)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("-------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("PRIMAR: " + prim_U.ToString("0.00") + " (V) / " + Math.Round(i_prim * K_U + p_gub_jez / prim_U, 3).ToString("0.000") + " (A)", font, XBrushes.Black, new XRect(50, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("-------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            if (brz_prim == 1) graph2.DrawString("Broj zavoja: " + n_prim_n.ToString(), font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            else graph2.DrawString("Broj zavoja: " + n_prim_n.ToString() + " x " + brz_prim.ToString() + " = " + (brz_prim * n_prim).ToString(), font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("Promjer žice: " + fi_prim.ToString("0.000") + " (mm)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("Broj žica: " + brz_prim.ToString(), font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("Duljina žice: " + lprim.ToString("0.00") + " (m) => " + RW2_PR.ToString() + " RW2(0) / " + RW3_PR.ToString() + " RW3(0)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("Debljina primara: " + Math.Round(dprim, 1).ToString("0.0") + " (mm) => " + sprim.ToString() + " slojeva", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("-------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("SEKUNDAR: " + sek_U.ToString("0.00") + " (V) / " + Math.Round(i_sek, 2).ToString("0.00") + " (A)", font, XBrushes.Black, new XRect(50, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("-------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            if (brz_sek == 1) graph2.DrawString("Broj zavoja: " + n_sek_n.ToString(), font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            else graph2.DrawString("Broj zavoja: " + n_sek_n.ToString() + " x " + brz_sek + " žica = " + (brz_sek * n_sek_n).ToString(), font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("Promjer žice: " + fi_sek.ToString("0.000") + " (mm)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("Broj žica: " + brz_sek.ToString(), font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("Duljina žice: " + lsek.ToString("0.00") + " (m) => " + RW2_SE.ToString() + " RW2(0) / " + RW3_SE + " RW3(0)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("Debljina sekundara: " + dsek.ToString("0.0") + " (mm) => " + ssek1 + " slojeva", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("-------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("DIMENZIJE NAMOTANOG TRANSFORMATORA", font, XBrushes.Black, new XRect(50, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("-------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("Vanjski promjer: " + Math.Round(suk_ko, 1).ToString("0.0") + " (mm)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("Unutarnji promjer: " + Math.Round(dost_ko, 1).ToString("0.0") + " (mm)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("Ukupna visina: " + Math.Round(vuk_ko, 1).ToString("0.0") + " (mm)", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);
            graph2.DrawString("-------------------------------------------------------------------------------------", font, XBrushes.Black, new XRect(lpoz, poz += 15, pdfPage2.Width.Point, pdfPage2.Height.Point), XStringFormats.TopLeft);

            string pdfFilename2 = System.IO.Path.Combine(desktop, "toroid_ispis_2.pdf");
            pdf2.Save(pdfFilename2);
            Process.Start(pdfFilename2);

            MessageBox.Show("PDF datoteke spremljene na radnu površinu.");
        }

        private void btnZagrijavanje_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrWhiteSpace(txtMaxGustocaStruje.Text))
            {
                if (!string.IsNullOrWhiteSpace(txtDozvoljenoZagrijavanje.Text) && !string.IsNullOrWhiteSpace(txtDebljinaNamotaja.Text))
                {
                    gpros = k * Math.Sqrt((dt / 2) / (1 + d_nam / 30) * (d_nam / 10));
                    txtMaxGustocaStruje.Text = gpros.ToString();
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtDozvoljenoZagrijavanje.Text))
                {
                    if (!string.IsNullOrWhiteSpace(txtDebljinaNamotaja.Text))
                    {
                        dt = Math.Pow(gpros / k, 2) * (1 + (d_nam / 30)) * (d_nam / 10) * 2;
                        lblDozvoljenoZagrijavanje.Text = "Temp. kod " + snaga.ToString("0.00") + "(VA) (°C)";
                        txtDozvoljenoZagrijavanje.Text = dt.ToString("0.0");
                    }
                }
                else
                {
                    gpros = k * Math.Sqrt((dt / 2) / ((1 + d_nam / 30) * (d_nam / 10)));
                    txtMaxGustocaStruje.Text = gpros.ToString();
                }
            }
        }
    }
}
