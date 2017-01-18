using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Etupirka
{
	public static class StringProcessing
	{

		static Dictionary<char, char> conv = new Dictionary<char, char>() {
	{'１','1'},{'２','2'},{'３','3'},{'４','4'},{'５','5'},
	{'６','6'},{'７','7'},{'８','8'},{'９','9'},{'０','0'},
	{'Ａ','A'},{'Ｂ','B'},{'Ｃ','C'},{'Ｄ','D'},{'Ｅ','E'},
	{'Ｆ','F'},{'Ｇ','G'},{'Ｈ','H'},{'Ｉ','I'},{'Ｊ','J'},
	{'Ｋ','K'},{'Ｌ','L'},{'Ｍ','M'},{'Ｎ','N'},{'Ｏ','O'},
	{'Ｐ','P'},{'Ｑ','Q'},{'Ｒ','R'},{'Ｓ','S'},{'Ｔ','T'},
	{'Ｕ','U'},{'Ｖ','V'},{'Ｗ','W'},{'Ｘ','X'},{'Ｙ','Y'},
	{'Ｚ','Z'},
	{'ａ','a'},{'ｂ','b'},{'ｃ','c'},{'ｄ','d'},{'ｅ','e'},
	{'ｆ','f'},{'ｇ','g'},{'ｈ','h'},{'ｉ','i'},{'ｊ','j'},
	{'ｋ','k'},{'ｌ','l'},{'ｍ','m'},{'ｎ','n'},{'ｏ','o'},
	{'ｐ','p'},{'ｑ','q'},{'ｒ','r'},{'ｓ','s'},{'ｔ','t'},
	{'ｕ','u'},{'ｖ','v'},{'ｗ','w'},{'ｘ','x'},{'ｙ','y'},
	{'ｚ','z'},
	{ '　',' '},{'～',' '},{'…',' '}, {'’',' '}, {'＝',' '},
			{'・',' '}, {'（',' ' }, {'）',' '}, {'－',' '}, {'．',' ' },
			{'〔',' ' }, {'〕', ' '}, {'☆',' '}, {'＃',' '}, {'＊',' ' },
			{'♪',' ' }, {'★', ' '}, {'％',' ' }, {'［',' '}, {'］',' ' }, {'「',' ' }, {'」',' ' }, {'【',' ' }, {'】', ' ' },
			{'×',' ' }, {'“',' ' }, {'”',' ' }, {'！',' '}, {'√',' ' },
			{'？',' ' }, {'￥',' '}, {'‘',' ' }, {'＠',' ' }, {'＾',' ' },
			{'＄',' ' }, {'：',' ' }, {'＜',' ' }, {'＞',' ' }, {'＆',' '},
			{'!',' ' }, {'~',' ' }, {'@',' ' }, {'#',' ' }, {'$',' ' }, {'%',' ' },
			{'^',' ' }, {'&',' ' }, {'*',' ' }, {'(',' ' }, {')',' ' }, {'`',' ' },
			{'-',' ' }, {'_',' ' }, {'+',' ' }, {'=',' ' }, {'{',' ' }, {'}',' '},
			{'\\',' ' }, {'\'',' ' }, {':',' ' }, {';',' ' }, {'|',' ' }, {'"',' ' },
			{'<',' ' }, {'>',' ' }, {',',' ' }, {'.',' ' }, {'?',' ' }, {'/',' ' }
		};
	
		static public string FormattingGameTitleString(string str)
		{
			string s = new string(str.Select(n => (conv.ContainsKey(n) ? conv[n] : n)).ToArray());
			s = s.ToLower();
			return Regex.Replace(s, @"\s+", "");
		}

		static public int LevenshteinDistance(string a0, string b0)
		{
			string a = FormattingGameTitleString(a0);
			string b = FormattingGameTitleString(b0);
			int lengthA = a.Length;
			int lengthB = b.Length;
			var distances = new int[lengthA + 1, lengthB + 1];
			for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
			for (int j = 0; j <= lengthB; distances[0, j] = j++) ;
			for (int i = 1; i <= lengthA; i++)
				for (int j = 1; j <= lengthB; j++)
				{
					int cost = b[j - 1] == a[i - 1] ? 0 : 5;
					distances[i, j] = Math.Min
						(
						Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
						distances[i - 1, j - 1] + cost
						);
				}
			return distances[lengthA, lengthB];
		}
	}

};
