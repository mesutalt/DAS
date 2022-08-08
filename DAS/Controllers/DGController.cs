using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.DirectoryServices;
using DAS.Models;
using System.Data.Entity;
using System.Web;
using System.IO;
namespace DAS.Controllers
{
    public class DGController : Controller
    {
        DAS_CONVERTEntities db = new DAS_CONVERTEntities();
        public ActionResult Altbaslik()
        {
            ViewBag.count = db.DG_ALTBASLIK.Count();
            var list = db.DG_ALTBASLIK.OrderBy(x => x.ALT_BASLIK).ToList();
            return View(list);
        }
        public ActionResult AltbaslikEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AltbaslikEkle(string name, DG_ALTBASLIK dG_ALTBASLIK)
        {
            dG_ALTBASLIK.ALT_BASLIK = name;
            db.DG_ALTBASLIK.Add(dG_ALTBASLIK);
            db.SaveChanges();
            return RedirectToAction("Altbaslik");
        }
        public ActionResult Altbaslikdelete(int ID)
        {
            DG_ALTBASLIK a = db.DG_ALTBASLIK.Where(x => x.ID == ID).FirstOrDefault();
            db.DG_ALTBASLIK.Remove(a);
            db.SaveChanges();
            return RedirectToAction("Altbaslik");
        }
        public ActionResult Programmatic()
        {
            ViewBag.count=db.DG_ANABASLIK_PROGRAMMATIC.Count();
            var list= db.DG_ANABASLIK_PROGRAMMATIC.OrderBy(x => x.ID).ToList();
            return View(list);
        }
        public ActionResult ProgrammaticEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ProgrammaticEkle(string name1, string name2, DG_ANABASLIK_PROGRAMMATIC dG_ANABASLIK_PROGRAMMATIC)
        {
            dG_ANABASLIK_PROGRAMMATIC.KATEGORI = name1;
            dG_ANABASLIK_PROGRAMMATIC.ANA_BASLIK = name2;
            db.DG_ANABASLIK_PROGRAMMATIC.Add(dG_ANABASLIK_PROGRAMMATIC);
            db.SaveChanges();
            return RedirectToAction("Programmatic");
        }
        public ActionResult Programmaticdelete(int ID)
        {
            DG_ANABASLIK_PROGRAMMATIC a=db.DG_ANABASLIK_PROGRAMMATIC.Where(x => x.ID == ID).FirstOrDefault();
            db.DG_ANABASLIK_PROGRAMMATIC.Remove(a);
            db.SaveChanges();
            return RedirectToAction("Programmatic");
        }
        public ActionResult Reserve()
        {
            ViewBag.count = db.DG_ANABASLIK_RESERVE.Count();
            var list=db.DG_ANABASLIK_RESERVE.OrderBy(x => x.ID).ToList();
            return View(list);
        }
        public ActionResult ReserveEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ReserveEkle(string name1, string name2, DG_ANABASLIK_RESERVE dG_ANABASLIK_RESERVE)
        {
            dG_ANABASLIK_RESERVE.KATEGORI = name1;
            dG_ANABASLIK_RESERVE.ANA_BASLIK = name2;
            db.DG_ANABASLIK_RESERVE.Add(dG_ANABASLIK_RESERVE);
            db.SaveChanges();
            return RedirectToAction("Reserve");
        }
        public ActionResult Reservedelete(int ID)
        {
            DG_ANABASLIK_RESERVE a =db.DG_ANABASLIK_RESERVE.Where(x => x.ID == ID).FirstOrDefault();
            db.DG_ANABASLIK_RESERVE.Remove(a);
            db.SaveChanges();
            return RedirectToAction("Reserve");
        }
        public ActionResult Search()
        {
            ViewBag.count=db.DG_ANABASLIK_SEARCH.Count();
            var list=db.DG_ANABASLIK_SEARCH.OrderBy(x => x.ID).ToList();
            return View(list);
        }
        public ActionResult SearchEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SearchEkle(string name1, string name2, DG_ANABASLIK_SEARCH dG_ANABASLIK_SEARCH)
        {
            dG_ANABASLIK_SEARCH.KATEGORI = name1;
            dG_ANABASLIK_SEARCH.ANA_BASLIK = name2;
            db.DG_ANABASLIK_SEARCH.Add(dG_ANABASLIK_SEARCH);
            db.SaveChanges();
            return RedirectToAction("Search");
        }
        public ActionResult Searchdelete(int ID)
        {
            DG_ANABASLIK_SEARCH a=db.DG_ANABASLIK_SEARCH.Where(x => x.ID == ID).FirstOrDefault();
            db.DG_ANABASLIK_SEARCH.Remove(a);
            db.SaveChanges();
            return RedirectToAction("Search");
        }
        public ActionResult Social()
        {
            ViewBag.count = db.DG_ANABASLIK_SOCIAL.Count();
            var list=db.DG_ANABASLIK_SOCIAL.OrderBy(x => x.ID).ToList();
            return View(list);
        }
        public ActionResult SocialEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SocialEkle(string name1, string name2, DG_ANABASLIK_SOCIAL dG_ANABASLIK_SOCIAL)
        {
            dG_ANABASLIK_SOCIAL.KATEGORI = name1;
            dG_ANABASLIK_SOCIAL.ANA_BASLIK = name2;
            db.DG_ANABASLIK_SOCIAL.Add(dG_ANABASLIK_SOCIAL);
            db.SaveChanges();
            return RedirectToAction("Social");
        }
        public ActionResult Socialdelete(int ID)
        {
            DG_ANABASLIK_SOCIAL a = db.DG_ANABASLIK_SOCIAL.Where(x => x.ID == ID).FirstOrDefault();
            db.DG_ANABASLIK_SOCIAL.Remove(a);
            db.SaveChanges();
            return RedirectToAction("Social");
        }
        public ActionResult TechCost()
        {
            ViewBag.count = db.DG_ANABASLIK_TECH_COST.Count();
            var list=db.DG_ANABASLIK_TECH_COST.ToList();
            return View(list);
        }
        public ActionResult TechCostEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult TechCostEkle(string name1, string name2, DG_ANABASLIK_TECH_COST dG_ANABASLIK_TECH_COST)
        {
            dG_ANABASLIK_TECH_COST.KATEGORI = name1;
            dG_ANABASLIK_TECH_COST.ANA_BASLIK = name2;
            db.DG_ANABASLIK_TECH_COST.Add(dG_ANABASLIK_TECH_COST);
            db.SaveChanges();
            return RedirectToAction("TechCost");
        }
        public ActionResult TechCostdelete(int ID)
        {
            DG_ANABASLIK_TECH_COST a=db.DG_ANABASLIK_TECH_COST.Where(x => x.ID == ID).FirstOrDefault();
            db.DG_ANABASLIK_TECH_COST.Remove(a);
            db.SaveChanges();
            return RedirectToAction("TechCost");
        }
        public ActionResult Vas()
        {
            ViewBag.count = db.DG_ANABASLIK_VAS.Count();
            var list = db.DG_ANABASLIK_VAS.ToList();
            return View(list);
        }
        public ActionResult VasEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult VasEkle(string name1, string name2, DG_ANABASLIK_VAS dG_ANABASLIK_VAS)
        {
            dG_ANABASLIK_VAS.KATEGORI = name1;
            dG_ANABASLIK_VAS.ANA_BASLIK = name2;
            db.DG_ANABASLIK_VAS.Add(dG_ANABASLIK_VAS);
            db.SaveChanges();
            return RedirectToAction("Vas");
        }
        public ActionResult Vasdelete(int ID)
        {
            DG_ANABASLIK_VAS a=db.DG_ANABASLIK_VAS.Where(x=>x.ID==ID).FirstOrDefault();
            db.DG_ANABASLIK_VAS.Remove(a);
            db.SaveChanges();
            return RedirectToAction("Vas");
        }
        public ActionResult Xaxis()
        {
            ViewBag.count=db.DG_ANABASLIK_XAXIS.Count();
            var list=db.DG_ANABASLIK_XAXIS.ToList();
            return View(list);
        }
        public ActionResult XaxisEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult XaxisEkle(string name1, string name2, DG_ANABASLIK_XAXIS dG_ANABASLIK_XAXIS)
        {
            dG_ANABASLIK_XAXIS.KATEGORI = name1;
            dG_ANABASLIK_XAXIS.ANA_BASLIK = name2;
            db.DG_ANABASLIK_XAXIS.Add(dG_ANABASLIK_XAXIS);
            db.SaveChanges();
            return RedirectToAction("Xaxis");
        }
        public ActionResult Xaxisdelete(int ID)
        {
            DG_ANABASLIK_XAXIS a=db.DG_ANABASLIK_XAXIS.Where(x => x.ID==ID).FirstOrDefault();
            db.DG_ANABASLIK_XAXIS.Remove(a);
            db.SaveChanges();
            return RedirectToAction("Xaxis");
        }
        public ActionResult Birim()
        {
            var list = db.DG_BIRIM.ToList();
            return View(list);
        }
        public ActionResult BirimEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult BirimEkle(string name,DG_BIRIM dG_BIRIM)
        {
            dG_BIRIM.KATEGORI = name;
            db.DG_BIRIM.Add(dG_BIRIM);
            db.SaveChanges();
            return RedirectToAction("Birim");
        }
        public ActionResult Birimdelete(int ID)
        {
            DG_BIRIM a=db.DG_BIRIM.Where(x => x.ID==ID).FirstOrDefault();
            db.DG_BIRIM.Remove(a);
            db.SaveChanges();
            return RedirectToAction("Birim");
        }
    }
}