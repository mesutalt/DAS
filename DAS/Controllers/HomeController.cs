using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.DirectoryServices;
using DAS.Models;
using System.Data.Entity;
using System.Web;
using System.IO;
using OfficeOpenXml;
using System.Data;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using System.Data.SqlClient;
using ClosedXML.Excel;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        DAS_CONVERTEntities db = new DAS_CONVERTEntities();
        private System.DirectoryServices.DirectoryEntry GetDirectoryObject(string USERNAME, string PASSWO)
        {
            DirectoryEntry newus = new DirectoryEntry();
            newus.Username = string.Format("{0}", USERNAME);
            newus.Password = string.Format("{0}", PASSWO);
            newus.Path = "LDAP://10.219.168.51";
            newus.AuthenticationType = AuthenticationTypes.Secure;
            return newus;
        }
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(string username, string pass)
        {
            if (username == null || pass == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                try
                {
                    DirectoryEntry de = GetDirectoryObject(username.ToString().Trim(), pass.ToString().Trim());
                    DirectorySearcher deSearch = new DirectorySearcher(de) { SearchRoot = de, Filter = string.Format("(&(objectClass=user)(SAMAccountName={0}))", username.ToString().Trim()) };
                    deSearch.PropertiesToLoad.Add("givenName");
                    deSearch.PropertiesToLoad.Add("userPrincipalName");
                    deSearch.PropertiesToLoad.Add("sAMAccountName");
                    deSearch.PropertiesToLoad.Add("displayName");

                    SortOption Srt = new SortOption("mail", SortDirection.Ascending);
                    deSearch.Sort = Srt;
                    var test = deSearch.FindAll();
                    SearchResultCollection Results = deSearch.FindAll();
                    if (Results != null)
                    {
                        foreach (SearchResult Result in Results)
                        {
                            ResultPropertyCollection Rpc = Result.Properties;
                            var email = Result.Properties["userPrincipalName"][0].ToString();
                            var uname = Result.Properties["displayName"][0].ToString();
                            var account = Result.Properties["sAMAccountName"][0].ToString();
                            Session["username"] = uname;
                            var userdt = db.USER_LIST.Where(x => x.USERNAME == account).FirstOrDefault();
                            if (userdt == null) return View("Index");
                            else
                            {
                                Session["account"] = userdt.USERNAME;
                                Session["rol"] = userdt.ROL;
                                Session["ajans"] = userdt.AJANS.Trim();
                            }
                        }
                        return RedirectToAction("Home", "Home");
                    }
                }
                catch (DirectoryServicesCOMException ex)
                {
                    if (ex.Data == null)
                    {
                        throw;
                    }
                    else
                    {
                        return View("Index", "Home");
                    }
                }
            }
            return View();
        }
        public ActionResult Home()
        {
            var client_kodlama = db.CLIENT_KODLAMA.OrderBy(x => x.ID).ToList();
            return View(client_kodlama);
        }

        public ActionResult Detail(int ID)
        {
            return View(db.CLIENT_KODLAMA.Where(x => x.ID == ID).FirstOrDefault());
        }
        public ActionResult Edit(int ID)
        {
            ViewBag.grup = new SelectList(db.MUSTERI_GRUPLARI.OrderBy(x => x.MUSTERI_GRUBU), "MUSTERI_GRUBU", "MUSTERI_GRUBU");
            //var a = (from p in db.CLIENT_KODLAMA
            //         orderby p.MUSTERI_GRUBU
            //         select db.CLIENT_KODLAMA).Distinct();
            return View(db.CLIENT_KODLAMA.Where(x=>x.ID ==ID).FirstOrDefault());
        }
        [HttpPost]
        public ActionResult Edit(CLIENT_KODLAMA cLIENT_KODLAMA)
        {
            try
            {
                db.Entry(cLIENT_KODLAMA).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch
            {
                return RedirectToAction("" + cLIENT_KODLAMA.ID, "Home/Edit");
            }
            return RedirectToAction("" + cLIENT_KODLAMA.ID, "Home/Detail");
        }
        public ActionResult Musteriekle()
        {
            ViewBag.grup = new SelectList(db.MUSTERI_GRUPLARI.OrderBy(x => x.MUSTERI_GRUBU), "MUSTERI_GRUBU", "MUSTERI_GRUBU");
            return View();
        }
        [HttpPost]
        public ActionResult Musteriekle(CLIENT_KODLAMA cLIENT_KODLAMA)
        {
            cLIENT_KODLAMA.MUSTERI_GRUBU_CONVERT = cLIENT_KODLAMA.MUSTERI_GRUBU.ToUpper();
            cLIENT_KODLAMA.MUSTERI_KODU_CONVERT= cLIENT_KODLAMA.MUSTERI_KODU.ToUpper();
            db.CLIENT_KODLAMA.Add(cLIENT_KODLAMA);
            db.SaveChanges();
            return RedirectToAction("Home");
        }
        [HttpPost]
        public ActionResult GrupEkle(string grup, MUSTERI_GRUPLARI mUSTERI_GRUPLARI)
        {
            var a = db.MUSTERI_GRUPLARI.Where(x => x.MUSTERI_GRUBU == grup).FirstOrDefault();
            if(a==null)
            {
                mUSTERI_GRUPLARI.MUSTERI_GRUBU = grup;
                db.MUSTERI_GRUPLARI.Add(mUSTERI_GRUPLARI);
                db.SaveChanges();
                return RedirectToAction("Musteriekle");
            }
            else
                return RedirectToAction("Musteriekle");
        }
        //public ActionResult GrupDelete(int ID)
        //{
        //    MUSTERI_GRUPLARI mUSTERI_GRUPLARI = db.MUSTERI_GRUPLARI.Where(x => x.ID == ID).FirstOrDefault();
        //    db.MUSTERI_GRUPLARI.Remove(mUSTERI_GRUPLARI);
        //    db.SaveChanges();
        //    return RedirectToAction("GrupEkle");
        //}
        public ActionResult Delete(int ID)
        {
            CLIENT_KODLAMA cLIENT = db.CLIENT_KODLAMA.Where(x => x.ID == ID).FirstOrDefault();
            db.CLIENT_KODLAMA.Remove(cLIENT);
            db.SaveChanges();
            return RedirectToAction("Home");
        }
        public ActionResult MedplanYukle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult MedplanYukle(HttpPostedFileBase file, MEDPLAN_DETAYLI_FATURA_DOKUMU a)
        {
            if (file==null)
            {
                ViewBag.hata = "Boş bırakmayınız.";
                return View();
            }
            else
            {
                if (a.AJANS == null)
                {
                    a.AJANS = (string)Session["ajans"];
                }
                string path = Path.Combine(Server.MapPath("~/document"), DateTime.Now.ToString().Replace(".", "-").Replace(" ", "-").Replace(":", "-"));
                file.SaveAs(path);
                using(var package = new ExcelPackage(file.InputStream))
                {
                    var currentsheet = package.Workbook.Worksheets;
                    var worksheet = currentsheet.Add("name");
                    var satir = worksheet.Dimension.End.Row;
                    var sutun = worksheet.Dimension.End.Column;
                    worksheet.InsertRow(1,1);
                    for (int i=4;i<=satir;i++)
                    {
                        //a.MECRA_TURU = worksheet.Cells[i, 1].Value.ToString();
                        //a.INTERNET_KATEGORI = worksheet.Cells[i, 2].Value?.ToString();
                        //a.INTERNET_RAPOR_ANA = worksheet.Cells[i, 3].Value?.ToString();
                        //a.INTERNET_RAPOR_ALT = worksheet.Cells[i, 4].Value?.ToString();
                        //a.SPOT_TIPI = worksheet.Cells[i, 5].Value?.ToString();
                        //a.MUSTERI_KODU= worksheet.Cells[i, 6].Value?.ToString();
                        //a.MUSTERI_GRUBU = worksheet.Cells[i, 7].Value?.ToString();
                        //a.MUSTERI_ADI = worksheet.Cells[i, 8].Value?.ToString();
                        //a.URUN_KODU = worksheet.Cells[i, 9].Value?.ToString();
                        //a.URUN_ADI = worksheet.Cells[i, 10].Value?.ToString();
                        //a.KAMPANYA = worksheet.Cells[i, 11].Value?.ToString();
                        //a.EKIP = worksheet.Cells[i, 12].Value?.ToString();
                        //a.IS_KOLU = worksheet.Cells[i, 13].Value?.ToString();
                        //a.PLAN_KODU = worksheet.Cells[i, 14].Value?.ToString();
                        //a.PAZARLAMA_SIRKETI = worksheet.Cells[i, 15].Value?.ToString();
                        //a.PAZARLAMA_SIRKETI_2 = worksheet.Cells[i, 16].Value?.ToString();
                        //a.YAYNI_GRUBU = worksheet.Cells[i, 17].Value?.ToString();
                        //a.SEHIR = worksheet.Cells[i, 18].Value?.ToString();
                        //a.MECRA_KODU = worksheet.Cells[i, 19].Value?.ToString();
                        //a.MECRA_ADI = worksheet.Cells[i, 20].Value?.ToString();
                        //a.DONEM = worksheet.Cells[i, 21].Value?.ToString();
                        //a.AYLAR = worksheet.Cells[i, 22].Value?.ToString();
                        //a.ADET = Convert.ToInt32(worksheet.Cells[i, 23].Value);
                        //a.SURE = Convert.ToInt32(worksheet.Cells[i, 24].Value);
                        //a.NET_TUTAR_DOLAR = Convert.ToDouble(worksheet.Cells[i, 25].Value);
                        //a.ORT_DOLAR_KURU = Convert.ToDouble(worksheet.Cells[i, 26].Value);
                        //a.MUSTERI_BRUT_TUTAR = Convert.ToDouble(worksheet.Cells[i, 27].Value);
                        //a.NET_TUTAR_TL = Convert.ToDouble(worksheet.Cells[i, 28].Value);
                        //a.INTERNET_SATIS_STOPAJ = Convert.ToDouble(worksheet.Cells[i, 29].Value);
                        //a.INTERNET_SATIS_STAPAJ_DAHIL = Convert.ToDouble(worksheet.Cells[i, 30].Value);
                        //a.INTERNET_ALIS_STOPAJ= Convert.ToDouble(worksheet.Cells[i, 31].Value);
                        //a.INTERNET_ALIS_STOPAJ_DAHIL= Convert.ToDouble(worksheet.Cells[i, 32].Value);
                        //a.MUSTERIDEN_GELEN_IADE = Convert.ToDouble(worksheet.Cells[i, 33].Value);
                        //a.MUSTERIDEN_GELEN_STOPAJ = Convert.ToDouble(worksheet.Cells[i, 34].Value);
                        //a.GERCEKLESEN_TUTAR = Convert.ToDouble(worksheet.Cells[i, 35].Value);
                        //a.GERCEKLESEN_TOPLAM = Convert.ToDouble(worksheet.Cells[i, 36].Value);
                        //a.AHB_TUTAR = Convert.ToDouble(worksheet.Cells[i, 37].Value);
                        //a.MUSTERIDEN_IADEN_GELEN_AHB = Convert.ToDouble(worksheet.Cells[i, 38].Value);
                        //a.GERCEKLESEN_AHB_TUTARI = Convert.ToDouble(worksheet.Cells[i, 39].Value);
                        //a.MUSTERI_IADE_FATURA_NO = worksheet.Cells[i, 40].Value?.ToString();
                        //if (a.MUSTERI_IADE_FATURA_TARIH != null)
                        //    a.MUSTERI_IADE_FATURA_TARIH = Convert.ToDateTime(worksheet.Cells[i, 41].Value);
                        //a.ONCEKI_YIL_MDV_ORANI = Convert.ToDouble(worksheet.Cells[i, 42].Value);
                        //a.ONCEKI_YIL_MDV_GERLIRI = Convert.ToDouble(worksheet.Cells[i, 43].Value);
                        //a.BU_YIL_MDV_ORANI = Convert.ToDouble(worksheet.Cells[i, 44].Value);
                        //a.BU_YIL_MDV_GELIRI = Convert.ToDouble(worksheet.Cells[i, 45].Value);
                        //a.MECRA_FATURA_TUTARI = Convert.ToDouble(worksheet.Cells[i, 46].Value);
                        //a.MECRA_FATURA_STOPAJ_TUTARI = Convert.ToDouble(worksheet.Cells[i, 47].Value);
                        //a.MECRA_FATURA_NO = worksheet.Cells[i, 48].Value?.ToString();
                        //a.MECRA_FATURA_TARIHI = worksheet.Cells[i, 49].Value?.ToString();
                        //a.MECRA_FATURA_PARA_BIRIM = worksheet.Cells[i, 50].Value?.ToString();
                        //a.MECRA_FATURA_TUTAR_DOVIZ = Convert.ToDouble(worksheet.Cells[i, 51].Value);
                        //a.IADE_TUTAR = Convert.ToDouble(worksheet.Cells[i, 52].Value);
                        //a.IADE_SONRASI_MECRA_FATURA_TUTARI = Convert.ToDouble(worksheet.Cells[i, 53].Value);
                        //a.MECRA_FARK_FATURA_TUTAR = Convert.ToDouble(worksheet.Cells[i, 54].Value);
                        //a.MECRA_FARK_FATURA_NO = worksheet.Cells[i, 55].Value?.ToString();
                        //a.MECRA_FARK_FATURA_TARIH = worksheet.Cells[i, 56].Value?.ToString();
                        //a.MECRA_FARK_FATURA_PARA = worksheet.Cells[i, 57].Value?.ToString();
                        //a.MECRA_FARK_FATURA_TUTARI_DOVIZ = Convert.ToDouble(worksheet.Cells[i, 58].Value);
                        //a.FARK_IADE_TUTARI = Convert.ToDouble(worksheet.Cells[i, 59].Value);
                        //a.FARK_IADE_SONRASI_FATURA_TUTARI = Convert.ToDouble(worksheet.Cells[i, 60].Value);
                        //a.IADELER_SONRASI_MECRA_FATURA_TOP = Convert.ToDouble(worksheet.Cells[i, 61].Value);
                        //a.SATIS_ALIS_FARK = Convert.ToDouble(worksheet.Cells[i, 62].Value);
                        //a.SATIS_ALIS_STOPAJ_FARK = Convert.ToDouble(worksheet.Cells[i, 63].Value);
                        //a.SATIS_ALIS_STOPAJ_FARK_TOP = Convert.ToDouble(worksheet.Cells[i, 64].Value);
                        //if(a.MECRA_IADE_FATURA_TARIH !=null)
                        //    a.MECRA_IADE_FATURA_TARIH = Convert.ToDateTime(worksheet.Cells[i, 65].Value);
                        //a.MECRA_IADE_FATURA_NO = worksheet.Cells[i, 66].Value?.ToString();
                        //if (a.MECRA_FARK_IADE_FATURA_TARIHI != null)
                        //    a.MECRA_FARK_IADE_FATURA_TARIHI = Convert.ToDateTime(worksheet.Cells[i, 67].Value);
                        //a.MECRA_FARK_IADE_FATURA_NO = worksheet.Cells[i, 68].Value?.ToString();
                        //a.ORJINAL_FATURA_NO_YAYIN = worksheet.Cells[i, 69].Value?.ToString();
                        //if (a.ORJINAL_FATURA_TARIH_YAYIN != null)
                        //    a.ORJINAL_FATURA_TARIH_YAYIN = Convert.ToDateTime(worksheet.Cells[i, 70].Value);
                        //a.ORJINAL_FATURA_TUTAR_YAYIN = Convert.ToDouble(worksheet.Cells[i, 71].Value);
                        //a.ORJINAL_FATURA_NO_FARK = worksheet.Cells[i, 72].Value?.ToString();
                        //if (a.ORJINAL_FATURA_TARIH_FARK != null)
                        //    a.ORJINAL_FATURA_TARIH_FARK = Convert.ToDateTime(worksheet.Cells[i, 73].Value);
                        //a.ORJINAL_FATURA_TUTAR_FARK = Convert.ToDouble(worksheet.Cells[i, 74].Value);
                        //a.ORJINAL_AVANS_FATURA_NO = worksheet.Cells[i, 75].Value?.ToString();
                        //if (a.ORJINAL_AVANS_FATURA_TARIH != null)
                        //    a.ORJINAL_AVANS_FATURA_TARIH = Convert.ToDateTime(worksheet.Cells[i, 76].Value);
                        //a.ORJINAL_AVANS_FATURA_TUTAR = Convert.ToDouble(worksheet.Cells[i, 77].Value);
                        //a.ORJINAL_FATURA_NO_AHB = worksheet.Cells[i, 78].Value?.ToString();
                        //if (a.ORJINAL_FATURA_TARIH_AHB != null)
                        //    a.ORJINAL_FATURA_TARIH_AHB = Convert.ToDateTime(worksheet.Cells[i, 79].Value);
                        //a.ORJINAL_FATURA_TUTAR_AHB = Convert.ToDouble(worksheet.Cells[i, 80].Value);
                        //a.ORJINAL_FATURA_TUTAR_STOPAJ = Convert.ToDouble(worksheet.Cells[i, 81].Value);
                        //a.ORJINAL_FATURA_NO_FARK_AHB = worksheet.Cells[i, 82].Value?.ToString();
                        //if (a.ORJINAL_FATURA_TARIH_FARK_AHB != null)
                        //    a.ORJINAL_FATURA_TARIH_FARK_AHB = Convert.ToDateTime(worksheet.Cells[i, 83].Value);
                        //a.ORJINAL_FATURA_TUTAR_FARK_AHB = Convert.ToDouble(worksheet.Cells[i, 84].Value);
                        //a.ORJINAL_AVANS_FATURA_NO_AHB = Convert.ToDouble(worksheet.Cells[i, 85].Value);
                        //if (a.ORJINAL_AVANS_FATURA_TARIH_AHB != null)
                        //    a.ORJINAL_AVANS_FATURA_TARIH_AHB = Convert.ToDateTime(worksheet.Cells[i, 86].Value);
                        //a.ORJINAL_AVANS_FATURA_TUTAR_AHB = Convert.ToDouble(worksheet.Cells[i, 87].Value);
                        //a.ORJINAL_FATURA_MUSTERISI = worksheet.Cells[i, 88].Value?.ToString();
                        //a.ORJINAL_FATURA_TUTAR_TOPLAM_YAYIN = Convert.ToDouble(worksheet.Cells[i, 89].Value);
                        //a.ORJINAL_FATURA_TUTAR_TOPLAM_AHB = Convert.ToDouble(worksheet.Cells[i, 90].Value);
                        //a.ORJINAL_FATURA_TUTAR_TOPLAM_YAYIN_STOPAJ = Convert.ToDouble(worksheet.Cells[i, 91].Value);
                        //a.ORJINAL_FATURA_TUTAR_TOPLAM = Convert.ToDouble(worksheet.Cells[i, 92].Value);
                        //a.TASLAK_FATURA_NO_YAYIN = worksheet.Cells[i, 93].Value?.ToString();
                        //if (a.TASLAK_FATURA_TARIH_YAYIN != null)
                        //    a.TASLAK_FATURA_TARIH_YAYIN = Convert.ToDateTime(worksheet.Cells[i, 94].Value);
                        //a.TASLAK_FATURA_NO_FARK_YAYIN = worksheet.Cells[i, 95].Value?.ToString();
                        //if (a.TASLAK_FATURA_TARIH_FARK_YAYIN != null)
                        //    a.TASLAK_FATURA_TARIH_FARK_YAYIN = Convert.ToDateTime(worksheet.Cells[i, 96].Value);
                        //a.TASLAK_AVANS_FATURA_NO_YAYIN = worksheet.Cells[i, 97].Value?.ToString();
                        //if (a.TASLAK_AVANS_FATURA_TARIH_YAYIN != null)
                        //    a.TASLAK_AVANS_FATURA_TARIH_YAYIN = Convert.ToDateTime(worksheet.Cells[i, 98].Value);
                        //a.TASLAK_AVANS_FATURA_TUTAR = Convert.ToDouble(worksheet.Cells[i, 99].Value);
                        //a.TASLAK_FATURA_NO_AHB = worksheet.Cells[i, 100].Value?.ToString();
                        //if (a.TASLAK_FATURA_TARIH_AHB != null)
                        //    a.TASLAK_FATURA_TARIH_AHB = Convert.ToDateTime(worksheet.Cells[i, 101].Value);
                        //a.TASLAK_FATURA_NO_FARK_AHB = worksheet.Cells[i, 102].Value?.ToString();
                        //if (a.TASLAK_FATURA_TARIH_FARK_AHB != null)
                        //    a.TASLAK_FATURA_TARIH_FARK_AHB = Convert.ToDateTime(worksheet.Cells[i, 103].Value);
                        //a.TASLAK_AVANS_FATURA_NO_AHB = worksheet.Cells[i, 104].Value?.ToString();
                        //if (a.TASLAK_AVANS_FATURA_TARIH_AHB != null)
                        //    a.TASLAK_AVANS_FATURA_TARIH_AHB = Convert.ToDateTime(worksheet.Cells[i, 105].Value);
                        //a.TASLAK_AVANS_FATURA_TUTAR_AHB = Convert.ToDouble(worksheet.Cells[i, 106].Value);
                        //a.AHB_ORANI = Convert.ToDouble(worksheet.Cells[i, 107].Value);
                        //a.ACIKLAMA = worksheet.Cells[i, 108].Value?.ToString();
                        //a.ALIM_SEKLI = worksheet.Cells[i, 109].Value?.ToString();
                        //a.YIL = Convert.ToInt32(worksheet.Cells[i, 110].Value);
                        //a.AY_NO = Convert.ToInt32(worksheet.Cells[i, 111].Value);
                        //a.RAPOR_ALIM_SEKLI = worksheet.Cells[i, 112].Value?.ToString();
                        //a.MECRA_FATURA_KIME_KESILIYOR = worksheet.Cells[i, 113].Value?.ToString();
                        //a.PLAN_NOTLARI = worksheet.Cells[i, 114].Value?.ToString();
                        //a.RAPOR_KATEGORISI = worksheet.Cells[i, 115].Value?.ToString();
                        //a.CNTENT = worksheet.Cells[i, 116].Value?.ToString();
                        //a.PO_NO = worksheet.Cells[i, 117].Value?.ToString();
                        //a.ALT_PO_NO = worksheet.Cells[i, 118].Value?.ToString();
                        //a.PO_TUTAR = Convert.ToDouble(worksheet.Cells[i, 119].Value);
                        //a.SATIS_FATURA_KAYNAK = worksheet.Cells[i, 120].Value?.ToString();
                        //a.PLAN_SON_KAYIT_BILGISI = worksheet.Cells[i, 121].Value?.ToString();
                        //a.PLAN_SON_KAYDEDEN_EPOSTA = worksheet.Cells[i, 122].Value?.ToString();
                        //a.PLAN_SYMPHONY_ID = worksheet.Cells[i, 123].Value?.ToString();
                        //a.SPOT_SYMPYONH_ID = worksheet.Cells[i, 124].Value?.ToString();
                        //a.ENVANTER_KAPSAM = worksheet.Cells[i, 125].Value?.ToString();

                        db.MEDPLAN_DETAYLI_FATURA_DOKUMU.Add(a);
                        db.SaveChanges();
                    }
                }
                ViewBag.succes = "Yükleme işlemi başarılı.";
                return View();
            }
        }
        public ActionResult AcikHava()
        {
            ViewBag.count = db.MASTER_ACIKHAVA_TURU.Count();
            var list= db.MASTER_ACIKHAVA_TURU.OrderBy(x => x.ACIKHAVA_TURU).ToList();
            return View(list);
        }
        public ActionResult Acikhavaekle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Acikhavaekle(MASTER_ACIKHAVA_TURU mASTER_ACIKHAVA_TURU)
        {
            db.MASTER_ACIKHAVA_TURU.Add(mASTER_ACIKHAVA_TURU);
            db.SaveChanges();
            return RedirectToAction("AcikHava");
        }
        public ActionResult Acikhavadelete(int ID)
        {
            MASTER_ACIKHAVA_TURU mASTER_ACIKHAVA_TURU =db.MASTER_ACIKHAVA_TURU.Where(x => x.ID == ID).FirstOrDefault();
            db.MASTER_ACIKHAVA_TURU.Remove(mASTER_ACIKHAVA_TURU);
            db.SaveChanges();
            return RedirectToAction("AcikHava");
        }
        public ActionResult BugdetType()
        {
            var list=db.MASTER_BUDGET_TYPE.OrderBy(x=>x.NAME).ToList();
            return View(list);
        }
        public ActionResult BugdetTypeEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult BugdetTypeEkle(MASTER_BUDGET_TYPE mASTER_BUDGET_TYPE)
        {
            mASTER_BUDGET_TYPE.BUDGET_TYPE_CONVERT = mASTER_BUDGET_TYPE.BUDGET_TYPE.ToUpper();
            db.MASTER_BUDGET_TYPE.Add(mASTER_BUDGET_TYPE);
            db.SaveChanges();
            return RedirectToAction("BugdetType");
        }
        public ActionResult BudgetDelete(int ID)
        {
            MASTER_BUDGET_TYPE mASTER_BUDGET_TYPE = db.MASTER_BUDGET_TYPE.Where(x => x.ID == ID).FirstOrDefault();
            db.MASTER_BUDGET_TYPE.Remove(mASTER_BUDGET_TYPE);
            db.SaveChanges();
            return RedirectToAction("BugdetType");
        }
        public ActionResult DigitalKodlama()
        {
            var list=db.MASTER_DIGITAL_KODLAMA.OrderByDescending(x => x.ID).ToList();
            return View(list);
        }
        public ActionResult DigitalKodlamaEkle()
        {
            ViewBag.grup = new SelectList(db.DIGITAL_KODLAMA_GRUPLARI.OrderBy(x => x.BIRIM), "BIRIM", "BIRIM");
            return View();
        }
        [HttpPost]
        public ActionResult DigitalKodlamaEkle(MASTER_DIGITAL_KODLAMA mASTER_DIGITAL_KODLAMA)
        {
            db.MASTER_DIGITAL_KODLAMA.Add(mASTER_DIGITAL_KODLAMA);
            db.SaveChanges();
            return RedirectToAction("DigitalKodlama");
        }
        public ActionResult DigitalEdit(int ID)
        {
            ViewBag.grup = new SelectList(db.DIGITAL_KODLAMA_GRUPLARI.OrderBy(x => x.BIRIM), "BIRIM", "BIRIM");
            return View(db.MASTER_DIGITAL_KODLAMA.Where(x=>x.ID==ID).FirstOrDefault());
        }
        [HttpPost]
        public ActionResult DigitalEdit(MASTER_DIGITAL_KODLAMA mASTER_DIGITAL_KODLAMA)
        {
            try
            {
                db.Entry(mASTER_DIGITAL_KODLAMA).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch
            {
                return RedirectToAction("" + mASTER_DIGITAL_KODLAMA.ID, "Home/Edit");
            }
            return RedirectToAction("DigitalKodlama");
        }
        public ActionResult DigitalDelete(int ID)
        {
            MASTER_DIGITAL_KODLAMA a=db.MASTER_DIGITAL_KODLAMA.Where(x => x.ID == ID).FirstOrDefault();
            db.MASTER_DIGITAL_KODLAMA.Remove(a);
            db.SaveChanges();
            return RedirectToAction("DigitalKodlama");
        }
        public ActionResult Birimekle(string grup, DIGITAL_KODLAMA_GRUPLARI dIGITAL_KODLAMA_GRUPLARI)
        {
            var a = db.DIGITAL_KODLAMA_GRUPLARI.Where(x => x.BIRIM == grup).FirstOrDefault();
            if (a == null)
            {
                dIGITAL_KODLAMA_GRUPLARI.BIRIM = grup;
                db.DIGITAL_KODLAMA_GRUPLARI.Add(dIGITAL_KODLAMA_GRUPLARI);
                db.SaveChanges();
                return RedirectToAction("DigitalKodlamaEkle");
            }
            else
            {
                return RedirectToAction("DigitalKodlamaEkle");
            }
        }
        public ActionResult TradingAjans()
        {
            var list = db.MASTER_MECRA_TRADING_AJANS.OrderBy(x => x.NAME).ToList();
            return View(list);
        }
        public ActionResult TradingAjansekle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult TradingAjansekle(MASTER_MECRA_TRADING_AJANS mASTER_MECRA_TRADING_AJANS)
        {
            mASTER_MECRA_TRADING_AJANS.AJANS_KODLANMIS = mASTER_MECRA_TRADING_AJANS.TRADING_KODLANMIS.ToUpper();
            db.MASTER_MECRA_TRADING_AJANS.Add(mASTER_MECRA_TRADING_AJANS);
            db.SaveChanges();
            return RedirectToAction("TradingAjans");
        }
        public ActionResult TradingDelete(int ID)
        {
            MASTER_MECRA_TRADING_AJANS a = db.MASTER_MECRA_TRADING_AJANS.Where(x => x.ID == ID).FirstOrDefault();
            db.MASTER_MECRA_TRADING_AJANS.Remove(a);
            db.SaveChanges();
            return RedirectToAction("TradingAjans");
        }
        public ActionResult MediaAgency()
        {
            var list = db.MASTER_MEDIA_AGENCY.OrderBy(x => x.NAME).ToList();
            ViewBag.count = db.MASTER_MEDIA_AGENCY.Count();
            return View(list);
        }
        public ActionResult MediaAgencyEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult MediaAgencyEkle(MASTER_MEDIA_AGENCY mASTER_MEDIA_AGENCY)
        {
            db.MASTER_MEDIA_AGENCY.Add(mASTER_MEDIA_AGENCY);
            db.SaveChanges();
            return RedirectToAction("MediaAgency");
        }
        public ActionResult MediaDelete(int ID)
        {
            MASTER_MEDIA_AGENCY a = db.MASTER_MEDIA_AGENCY.Where(x => x.ID == ID).FirstOrDefault();
            db.MASTER_MEDIA_AGENCY.Remove(a);
            db.SaveChanges();
            return RedirectToAction("MediaAgency");
        }
        public ActionResult Vehicle()
        {
            var list = db.MASTER_MEDIA_VEHICLE_TYPE.OrderBy(x => x.VALIDATION).ToList();
            ViewBag.count = db.MASTER_MEDIA_VEHICLE_TYPE.Count();
            return View(list);
        }
        public ActionResult VehicleEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult VehicleEkle(MASTER_MEDIA_VEHICLE_TYPE mASTER_MEDIA_VEHICLE_TYPE)
        {
            mASTER_MEDIA_VEHICLE_TYPE.MEDIA_TYPE = mASTER_MEDIA_VEHICLE_TYPE.NAME.ToUpper();
            db.MASTER_MEDIA_VEHICLE_TYPE.Add(mASTER_MEDIA_VEHICLE_TYPE);
            db.SaveChanges();
            return RedirectToAction("Vehicle");
        }
        public ActionResult VehicleDelete(int ID)
        {
            MASTER_MEDIA_VEHICLE_TYPE a = db.MASTER_MEDIA_VEHICLE_TYPE.Where(x => x.ID == ID).FirstOrDefault();
            db.MASTER_MEDIA_VEHICLE_TYPE.Remove(a);
            db.SaveChanges();
            return RedirectToAction("Vehicle");
        }
        public ActionResult OutdoorKodlama()
        {
            var list = db.MASTER_OUTDOOR_KODLAMA.OrderBy(x => x.MECRA).ToList();
            return View(list);
        }
        public ActionResult OutdoorKodlamaEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult OutdoorKodlamaEkle(string name1, string name2, MASTER_OUTDOOR_KODLAMA mASTER_OUTDOOR_KODLAMA)
        {
            mASTER_OUTDOOR_KODLAMA.MECRA = name1;
            mASTER_OUTDOOR_KODLAMA.MECRA_KODU = name2;
            mASTER_OUTDOOR_KODLAMA.MECRA_KODU_CONVERT = name2.ToUpper();
            db.MASTER_OUTDOOR_KODLAMA.Add(mASTER_OUTDOOR_KODLAMA);
            db.SaveChanges();
            return RedirectToAction("OutdoorKodlama");
        }
        public ActionResult OutdoorDelete(int ID)
        {
            MASTER_OUTDOOR_KODLAMA a = db.MASTER_OUTDOOR_KODLAMA.Where(x => x.ID == ID).FirstOrDefault();
            db.MASTER_OUTDOOR_KODLAMA.Remove(a);
            db.SaveChanges();
            return RedirectToAction("OutdoorKodlama");
        }
        public ActionResult Purchase()
        {
            var list=db.MASTER_PURCHASE_TYPE.OrderBy(x=>x.PURCHASE_TYPE).ToList();
            ViewBag.count = db.MASTER_PURCHASE_TYPE.Count();
            return View(list);
        }
        public ActionResult PurchaseEkle()
        { 
            return View();
        }
        [HttpPost]
        public ActionResult PurchaseEkle(string name1, string name2, MASTER_PURCHASE_TYPE mASTER_PURCHASE_TYPE)
        {
            mASTER_PURCHASE_TYPE.PURCHASE_TYPE = name1;
            mASTER_PURCHASE_TYPE.PURCHASE_NAME = name2;
            mASTER_PURCHASE_TYPE.PURCHASE_CONVERT = name2.ToUpper();
            db.MASTER_PURCHASE_TYPE.Add(mASTER_PURCHASE_TYPE);
            db.SaveChanges();
            return RedirectToAction("Purchase");
        }
        public ActionResult PurchaseDelete(int ID)
        {
            MASTER_PURCHASE_TYPE a = db.MASTER_PURCHASE_TYPE.Where(x => x.ID == ID).FirstOrDefault();
            db.MASTER_PURCHASE_TYPE.Remove(a);
            db.SaveChanges();
            return RedirectToAction("Purchase");
        }
        public ActionResult RadyoMecra()
        {
            var list = db.MASTER_RADYO_PAZSIRK_MECRA.OrderBy(x => x.PAZARLAMA_SIRKETI).ToList();
            return View(list);
        }
        public ActionResult RadyoMecraEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult RadyoMecraEkle(string name1, string name2, MASTER_RADYO_PAZSIRK_MECRA mASTER_RADYO_PAZSIRK_MECRA)
        {
            mASTER_RADYO_PAZSIRK_MECRA.PAZARLAMA_SIRKETI = name1;
            mASTER_RADYO_PAZSIRK_MECRA.MECRA = name2;
            mASTER_RADYO_PAZSIRK_MECRA.MECRA_CONVERT = name2.ToUpper();
            db.MASTER_RADYO_PAZSIRK_MECRA.Add(mASTER_RADYO_PAZSIRK_MECRA);
            db.SaveChanges();
            return RedirectToAction("RadyoMecra");
        }
        public ActionResult RadyoMecraDelete(int ID)
        {
            MASTER_RADYO_PAZSIRK_MECRA a = db.MASTER_RADYO_PAZSIRK_MECRA.Where(x => x.ID == ID).FirstOrDefault();
            db.MASTER_RADYO_PAZSIRK_MECRA.Remove(a);
            db.SaveChanges();
            return RedirectToAction("RadyoMecra");
        }
        public ActionResult RadyoPazarlama()
        {
            var list = db.MASTER_RADYO_PAZSIRK_PAZSIRK.OrderBy(x => x.RADYO_NAME).ToList();
            return View(list);
        }
        public ActionResult RadyoPazarlamaEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult RadyoPazarlamaEkle(string name1, string name2, MASTER_RADYO_PAZSIRK_PAZSIRK mASTER_RADYO_PAZSIRK_PAZSIRK)
        {
            mASTER_RADYO_PAZSIRK_PAZSIRK.RADYO_NAME = name1;
            mASTER_RADYO_PAZSIRK_PAZSIRK.RADYO_NAME_EDIT = name2;
            mASTER_RADYO_PAZSIRK_PAZSIRK.RADYO_NAME_CONVERT = name2.ToUpper();
            db.MASTER_RADYO_PAZSIRK_PAZSIRK.Add(mASTER_RADYO_PAZSIRK_PAZSIRK);
            db.SaveChanges();
            return RedirectToAction("RadyoPazarlama");
        }
        public ActionResult RadyoPazarlamaDelete(int ID)
        {
            MASTER_RADYO_PAZSIRK_PAZSIRK a = db.MASTER_RADYO_PAZSIRK_PAZSIRK.Where(x => x.ID == ID).FirstOrDefault();
            db.MASTER_RADYO_PAZSIRK_PAZSIRK.Remove(a);
            db.SaveChanges();
            return RedirectToAction("RadyoPazarlama");
        }
        public ActionResult Sehir()
        {
            var list = db.MASTER_SEHIR.OrderBy(x => x.SEHIR).ToList();
            return View(list);
        }
        public ActionResult SehirEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SehirEkle(string sehir, MASTER_SEHIR mASTER_SEHIR)
        {
            mASTER_SEHIR.SEHIR = sehir;
            mASTER_SEHIR.SEHIR_MASTER = sehir.ToUpper();
            db.MASTER_SEHIR.Add(mASTER_SEHIR);
            db.SaveChanges();
            return RedirectToAction("Sehir");
        }
        public ActionResult Sehirdelete(int ID)
        {
            MASTER_SEHIR a= db.MASTER_SEHIR.Where(x => x.ID == ID).FirstOrDefault();
            db.MASTER_SEHIR.Remove(a);
            db.SaveChanges();
            return RedirectToAction("Sehir");
        }
        public ActionResult Sponsorship()
        {
            ViewBag.count = db.MASTER_SPONSORSHIP.Count();
            var list=db.MASTER_SPONSORSHIP.OrderBy(x=>x.NAME).ToList();
            return View(list);
        }
        public ActionResult SponsorshipEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SponsorshipEkle(string name1, string name2, MASTER_SPONSORSHIP mASTER_SPONSORSHIP)
        {
            mASTER_SPONSORSHIP.NAME = name1;
            mASTER_SPONSORSHIP.VALIDATION = name2;
            db.MASTER_SPONSORSHIP.Add(mASTER_SPONSORSHIP);
            db.SaveChanges();
            return RedirectToAction("Sponsorship");
        }
        public ActionResult Sponsorshipdelete(int ID)
        {
            MASTER_SPONSORSHIP a= db.MASTER_SPONSORSHIP.Where(x=>x.ID == ID).FirstOrDefault();
            db.MASTER_SPONSORSHIP.Remove(a);
            db.SaveChanges();
            return RedirectToAction("Sponsorship");
        }
        public ActionResult YerelKodlama()
        {
            ViewBag.count = db.MASTER_YEREL_KODLAMA.Count();
            var list = db.MASTER_YEREL_KODLAMA.OrderBy(x => x.PAZARLAMA_SIRKETI).ToList();
            return View(list);
        }
        public ActionResult YerelKodlamaEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult YerelKodlamaEkle(string name1, string name2, MASTER_YEREL_KODLAMA mASTER_YEREL_KODLAMA)
        {
            mASTER_YEREL_KODLAMA.PAZARLAMA_SIRKETI = name1;
            mASTER_YEREL_KODLAMA.MECRA_KODU = name2;
            db.MASTER_YEREL_KODLAMA.Add(mASTER_YEREL_KODLAMA);
            db.SaveChanges();
            return RedirectToAction("YerelKodlama");
        }
        public ActionResult YerelKodlamadelete(int ID)
        {
            MASTER_YEREL_KODLAMA a=db.MASTER_YEREL_KODLAMA.Where(x => x.ID == ID).FirstOrDefault();
            db.MASTER_YEREL_KODLAMA.Remove(a);
            db.SaveChanges();
            return RedirectToAction("YerelKodlama");
        }
        public ActionResult MecraKodlama()
        {
            var list=db.MECRA_KODLAMA.OrderBy(x => x.ID).ToList();
            return View(list);
        }
        public ActionResult MecraKodlamaEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult MecraKodlamaEkle(MECRA_KODLAMA mECRA_KODLAMA)
        {
            mECRA_KODLAMA.MECRA_KODU_CONVERT = mECRA_KODLAMA.MECRA_KODU;
            db.MECRA_KODLAMA.Add(mECRA_KODLAMA);
            db.SaveChanges();
            return RedirectToAction("MecraKodlama");
        }
        public ActionResult MecraKodlamaDelete(int ID)
        {
            MECRA_KODLAMA a= db.MECRA_KODLAMA.Where(x => x.ID == ID).FirstOrDefault();
            db.MECRA_KODLAMA.Remove(a);
            db.SaveChanges();
            return RedirectToAction("MecraKodlama");
        }
        public ActionResult PazarlamaSirketi()
        {
            var list=db.PAZARLAMASIRKETI_KODLANMIS.OrderBy(x => x.ID).ToList();
            return View(list);
        }
        public ActionResult PazarlamaSirketiEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult PazarlamaSirketiEkle(string name1, string name2, PAZARLAMASIRKETI_KODLANMIS pAZARLAMASIRKETI_KODLANMIS)
        {
            pAZARLAMASIRKETI_KODLANMIS.MECRA_ADI = name2;
            pAZARLAMASIRKETI_KODLANMIS.AJANS_MECRA_ADI = name2;
            pAZARLAMASIRKETI_KODLANMIS.PAZARLAMA_SIRKETI = name1;
            db.PAZARLAMASIRKETI_KODLANMIS.Add(pAZARLAMASIRKETI_KODLANMIS);
            db.SaveChanges();
            return RedirectToAction("PazarlamaSirketi");
        }
        public ActionResult PazarlamaSirketiDelete(int ID)
        {
            PAZARLAMASIRKETI_KODLANMIS a = db.PAZARLAMASIRKETI_KODLANMIS.Where(x => x.ID == ID).FirstOrDefault();
            db.PAZARLAMASIRKETI_KODLANMIS.Remove(a);
            db.SaveChanges();
            return RedirectToAction("PazarlamaSirketi");
        }

        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        //public ActionResult trst( MEDPLAN_DETAYLI_FATURA_DOKUMU a)
        //{
        //    using (SqlConnection cn = new SqlConnection(_GLOBAL_PARAMETRELER._CONNECTION_STRING.ToString()))
        //    {
        //        cn.Open();
        //        using (SqlBulkCopy copy = new SqlBulkCopy(cn))
        //        {
        //            copy.ColumnMappings.Clear();
        //            for (int i = 0; i <= csvData.Columns.Count - 1; i++)
        //            {
        //                copy.ColumnMappings.Add(i, i);
        //            }
        //            copy.BulkCopyTimeout = 0;
        //            copy.DestinationTableName = "_TEMP_ADEX_DATA";
        //            copy.WriteToServer(a);
        //        }
        //    }
        //    return View();
        //}
    }
}