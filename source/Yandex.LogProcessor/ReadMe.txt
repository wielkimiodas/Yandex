-wczytujesz test do memstreama (memstream1)
-wczytujesz grupe z clickAnalyzera do list, tablic, itp
-tworzysz nowy memstream (memstream2)
-do InputFileOpener przekazujesz memstream1, do klasy dziedziczacej po InputFileReader przekazujesz wczytana grupe i memstream2
-dla kazdego query zmieniasz kolejnosc, jezeli uzytkownik obecnej sesji nalezy do grupy
-zapisujesz wszystko, co przyjdzie z memstream1 (przez onMetadata, onQuery i onClick) do memstream2 
	w kasie dziedziczacej po InputFileReader musisz utworzyc BinaryWritera (jako parametr podajac memstream2) i wszystkie obiekty (Click, Metadata, Query) maja metode WriteToFile
-wywalasz memstream1, memstream2 staje sie memstream1
-wczytujesz kolejna grupe

(jak bedziesz zmienial kolejnosc urli w otrzymanym obiekcie Query chyba nic zlego sie nie powinno wydarzyc)