in:
// ranking urli odczytany z pliku
urls : int[10]
// abstrakcyjne odwzorowanie urli na ich waznosc
// (w zaleznosci od grupy usera)
ranking : Map<int, float>

var:
wagaRankingu : float
wagaDefaulta : float
waznosc : List<Tuple<int, float>

for (int i = 0; i < urls.Length; i++)
  // wyliczenie defaultowego rankingu
  tmpWaznosc = (10-i) * wagaDefaulta;
  // ew modyfikacja waznosci urla
  if (ranking.ContainsKey(urls[i]))
    tmpWaznosc += ranking[urls[i]] * wagaRankingu;
  waznosc.Add(new Tuple(i, tmpWaznosc))

// przesortowanie kolejnosci zgodnie z wyliczonym rankingiem
waznosc.Sort((o1, o2) => o2.Item2.CompareTo(o1.Item2));

// zamiana kolejnosci w urls i zapisanie wyniku do outputa