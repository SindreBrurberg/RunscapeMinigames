using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Security;

namespace RunscapeMinigames
{
	class team {
		string name;
		List<user> users;
		public int points {get;}
		public team (string name, List<user> users) {
			this.name = name;
			this.users = users;
			points = 0;
			foreach (user user in users) {
				points += user.points;
			}
		}
		public string getTeam() {
			string retur = name;
			foreach (user user in users) {
				retur += " " + user.name;
			}
			return retur;
		}
		public int getPoints() {
			int s ;
			return points;
		}
	}
	class user {
		bool disqyalified = false;
		string skill, disqualifyingReason;
		int totalXP, eventXP, level;
		public int points {get;}
		public string name {get;}
		public user (string name, string disqualifyingReason) {
			this.name = name;
			this.disqualifyingReason = disqualifyingReason;
			disqyalified = true;
		}
		public user (string name, string skill, int totalXP, int eventXP, int points, int level) {
			this.name = name;
			this.skill = skill;
			this.totalXP = totalXP;
			this.eventXP = eventXP;
			this.points = points;
			this.level = level;
		}
		public string getUser() {
			if (disqyalified) {
				return (name + disqualifyingReason);
			} else {
				return (name + ": Skill: " + skill + " TotalXP: " + totalXP + " EventXP: " + eventXP + " Points: " + points + " Level: " + level);
			}
		}
	}
    class RunscapeMinigames
    {
		private static List<user> usersInfo = new List<user>();
		private static List<string> usernames = new List<string>();
		private static List<team> teams = new List<team>();
        static void Main(string[] args)
        {
			if (args.Length != 1) {return;}
			int id = Int16.Parse(args.First());
			if (id < 1) {return;}
			// Console.WriteLine(System.Security.Principal.WindowsIdentity.GetCurrent().Name);
			// using (StreamWriter file = new StreamWriter(
			//   new FileStream(@"C:\RunescapeMinigames\Competition " + id + ".txt", FileMode.Create))){
			// 	file.WriteLine("TEST");   
			// 	Console.WriteLine("Written to file");
			// }
			// return;
            string ds=MakeAsyncRequest("http://www.runeclan.com/clan/Consentus/competitions?id=" + id, "text/html").Result;
			while (ds.Contains("database error")) {
				ds = MakeAsyncRequest("http://www.runeclan.com/clan/Consentus/competitions?id=" + id, "text/html").Result;
			}
			// Console.WriteLine("Starting first Thread");
            Thread th = new Thread(()=>getUserInfo(ds));
            th.Start();
            int numOfPages = Int16.Parse(ds.Substring(ds.IndexOf("<div class=\"pagination_page\">Page 1 of ") + "<div class=\"pagination_page\">Page 1 of ".Length) 
				.Remove(ds.IndexOf("</div><div class=\"pagination_select\"><span class=\"disabled\">") - 
				ds.IndexOf("<div class=\"pagination_page\">Page 1 of ") - "<div class=\"pagination_page\">Page 1 of ".Length));
            Thread[] tha = new Thread[numOfPages-1];
            if (numOfPages > 1) {
				for (int i = 2; i<=numOfPages; i++) {
					Console.WriteLine("http://www.runeclan.com/clan/Consentus/competitions?id=" + id + "&page=" + i);
                    ds=MakeAsyncRequest("http://www.runeclan.com/clan/Consentus/competitions?id=" + id + "&page=" + i, "text/html").Result;
					while (ds.Contains("database error")) {
						ds = MakeAsyncRequest("http://www.runeclan.com/clan/Consentus/competitions?id=" + id + "&page=" + i, "text/html").Result;
					}
					// Console.WriteLine("Starting new Thread");
                    tha[i-2] = new Thread(()=>getUserInfo(ds));
                    tha[i-2].Start();
                }
            }
			string forum = MakeAsyncRequest("http://consentus.co.uk/events/skilling-clan-competition-12206/", "text/html").Result;
			string inner = forum.Substring(forum.IndexOf("Teams currently signed up:")).Remove(forum.IndexOf("ATTENTION!")-forum.IndexOf("Teams currently signed up:"));
			List<string> teamInfo = new List<string>();
			foreach (string span in inner.Split(new string[] {"</span>"}, StringSplitOptions.None)) {
				string s = span.Split(new string[] {". "}, StringSplitOptions.None).Last().Split(new string[] {"\">"}, StringSplitOptions.None).Last();
				if (s != "" && s != "<br />" && s != "Teams currently signed up:") {
					teamInfo.Add(s);
				}

			}
			if (numOfPages > 1) {
				for (int i = 2; i<=numOfPages; i++) {
                    tha[i-2].Join();
                }
			}
			th.Join();
			// foreach (string username in usernames) {
			// 	if (username =)
			// }
			// for (int i = 0; i < teamInfo.Count; i+=2) {
			// 	List<user> teamUsers = new List<user>();
			// 	foreach (string teamUser in teamInfo[i+1].Split(new string[] {" and "}, StringSplitOptions.None)) {
			// 		foreach (string username in usernames) {
			// 			if (username == teamUser) {
			// 				Console.WriteLine(username);
			// 			}
			// 		}
			// 		Console.WriteLine(teamUser);
			// 		Console.WriteLine(usernames.IndexOf("RubMyBeard"));
			// 		teamUsers.Add(usersInfo[1]);
			// 		// teamUsers.Add(usersInfo[usersInfo.FindIndex(a => a.name == teamUser)-1]);
			// 	}
			// 	teams.Add(new team(teamInfo[i], teamUsers));
			// }
			// foreach (team team in teams.OrderBy(t=>t.points)) {
            //     Console.WriteLine(team.getTeam());
            // }
			using (StreamWriter file = new StreamWriter(
			  new FileStream(@"C:\RunescapeMinigames\Competition " + id + ".txt", FileMode.Create))){
				foreach (user user in usersInfo.OrderBy(user=>user.points)) {
                	if (user.points > 0) {
						string userString = user.getUser();
						file.WriteLine(userString);
					}
            	}     
			}
            // Console.WriteLine ("Got response of {0}", task.Result);
        }
        private static void getUserInfo(string page) {
            string eventPage = page.Substring(page.IndexOf("<div class=\"events_wrap\">")).Remove(page.IndexOf("<div class=\"page_footer\">") - page.IndexOf("<div class=\"events_wrap\">"));
			int Sub = eventPage.IndexOf("class=\"regular\">"); 
			int Remove = eventPage.LastIndexOf("<div class=\"pagination\">");
			string isss = "<td class=\"events_table2\">";
			int iss = eventPage.IndexOf(isss);
			int isend = eventPage.IndexOf("\" /><span class=\"competition_skillname");
			string skill = eventPage.Substring(iss + isss.Length).Remove(isend - iss- isss.Length).Substring(eventPage.IndexOf("alt=\"") + 5 - iss - isss.Length);
			foreach (string i in eventPage.Substring(Sub).Remove(Remove - Sub).Split(new string[] {"tr>"}, StringSplitOptions.None)) {
				if (i.Contains("td")) {
					string isns = "<a href=\"/user/";
					int isn = i.IndexOf(isns);
					int ien = i.IndexOf("\" name=");
					string username = i.Substring(isn + isns.Length).Remove(ien - isn - isns.Length).Replace("+"," ");
					// Console.WriteLine(username);
					string ises = "class=\"clan_td clan_xpgain_trk\">";
					int ise = i.IndexOf(ises);
					int iee = i.IndexOf("</td></");
					int  eventXP = Int32.Parse(i.Substring(ise + ises.Length).Remove(iee - ise - ises.Length).Replace(",",""));
					String dsUser = MakeAsyncRequest("http://www.runeclan.com/user/"+ username.Replace(" ","+"), "text/html").Result;
					while (dsUser.Contains("database error")) {
						dsUser = MakeAsyncRequest("http://www.runeclan.com/user/"+ username.Replace(" ","+"), "text/html").Result;
					}
					// Console.WriteLine(dsUser.IndexOf("alt=\"Private Profile\""));
					if (dsUser.IndexOf("alt=\"Private Profile\"") != -1) {
						// userInfo.Add(username + ": Skill: " + skill + " PRIVATE PROFILE");
						usersInfo.Add(new user(username, "PRIVATE PROFILE"));
					} else if (dsUser.IndexOf("We are not currently tracking this users stats.") != -1) {
						usersInfo.Add(new user(username, " Not Tracking"));
					} else {
						string isdss = "onmousemove=\"xpTrackerBox('" + skill.ToLower();
						int isds = dsUser.IndexOf(isdss);
						int ieds = dsUser.LastIndexOf("<div class=\"adlgleaderboard");
						string[] skillArray;
						try {skillArray = dsUser.Substring(isds + isdss.Length).Remove(ieds - isds - isdss.Length).Split(new string[] {"td>"}, StringSplitOptions.None);}
						catch {
							Console.WriteLine(dsUser);
						}
						skillArray = dsUser.Substring(isds + isdss.Length).Remove(ieds - isds - isdss.Length).Split(new string[] {"td>"}, StringSplitOptions.None);
                    	// Console.Write(skillArray.Length + ": " + username);
						// if (skillArray.Length == 1) {Console.Write(": "+skillArray[0]);}
						// Console.WriteLine(": " + skillArray[1] + ": " + isds + ": " + ieds);
						int totalXP = Int32.Parse(skillArray[1].Replace("<td class=\"xp_tracker_cxp\">", "").Trim('/').Trim('<').Replace(",",""));
						usersInfo.Add(new user(username, skill, totalXP, eventXP, points(skill, totalXP, eventXP), getLevel(totalXP)));
						usernames.Add(username);
					}
				}
            }
        }
        public static Task<string> MakeAsyncRequest(string url, string contentType)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = contentType;
            request.Proxy = null;

            Task<WebResponse> task = Task.Factory.FromAsync(
                request.BeginGetResponse,
                asyncResult => request.EndGetResponse(asyncResult),
                (object)null);

            return task.ContinueWith(t => ReadStreamFromResponse(t.Result));
        }
        private static int xpForLevel(int level) {
			double total = 0;
			for (int i = 1; i < level; i++) {
				total += Math.Floor(i + 300 * Math.Pow(2, i /7.0));
			}
			return (int)Math.Floor(total / 4);
		}	
		private static int getLevel (int xp) {
			if (xpForLevel(99) < xp) {return 99;}
			for (int i = 1; i < 100; i++) {
				if (xpForLevel(i) > xp) { //kan bome med 1 xp
					return i-1;
				}
			}
			return 0;
		}
		private static int getPoints(int[] tier, int[] tierUp, int totalUserXP, int gainedXP) {
			int startXP = totalUserXP - gainedXP;
			int maxXPPH = tier[tier.Length - 1];
			int maxTiers = tierUp.Length;
			int startTier = 0;
			int tiers = 0;
			int points = 0;
			int XPLeft = gainedXP;
			for (int i = 0; i < maxTiers; i++) {
				if (startXP >= xpForLevel(tierUp[i])) {startTier++;}
				if (totalUserXP >= xpForLevel(tierUp[i])) {tiers++;}
			}
			for (int i = startTier; i < maxTiers; i++) {
				if (totalUserXP >= xpForLevel(tierUp[i])) {tiers++;}
			}
			for (int i = startTier; i <= tiers; i++) {
				if (startTier == tiers) {
					return (int)((double)maxXPPH/tier[i]/100.0*gainedXP);
				} else {
					if (i==tiers) {
						points += (int)((double)maxXPPH/tier[i]/100.0*XPLeft);
						return points;
					}
					Console.WriteLine(xpForLevel(tierUp[i]) - startXP);
					XPLeft -= xpForLevel(tierUp[i]) - startXP;
					points += (int)((double)maxXPPH/tier[i]/100.0*(xpForLevel(tierUp[i]) - startXP));
				}
			}
			return points;
		}
		private static int points (string skillName, int totalUserXP, int gainedXP) {
			switch (skillName) {
				case "Ovreall":
					break;
				case "Attack":
					break;
				case "Defence":
					break;
				case "Strength":
					break;
				case "Constitution":
					break;
				case "Ranged":
					break;
				case "Prayer":
					break;
				case "Magic":
					break;
				case "Cooking":
					break;
				case "Woodcutting":
					return getPoints(new int[4] {15000, 36000, 72000, 100000},new int[3] {30, 68, 94}, totalUserXP, gainedXP);
					break;
				case "Runecrafting":
					return getPoints(new int[4] {27000, 47000, 63000, 93000}, new int[3] {33, 66, 90}, totalUserXP, gainedXP);
					break;
				case "Divination":
					return getPoints(new int[12] {4000, 8000, 11000, 15000, 23000, 27000, 44000, 66000, 54000, 65000, 71000, 80000}, new int[11] {10, 20, 30, 40, 50, 60, 65, 75, 80, 90, 95}, totalUserXP, gainedXP);
					break;
			}
			return 1;
		}
        private static string ReadStreamFromResponse(WebResponse response)
        {
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader sr = new StreamReader(responseStream))
            {
                //Need to return this response 
                string strContent = sr.ReadToEnd();
                return strContent;
            }
        }
    }
}
