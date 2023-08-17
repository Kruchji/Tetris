# Uživatelská dokumentace ke hře Tetris

Program umožňuje hrát základní verzi hry Tetris. Cílem hry je získat co nejvyššího skóre pokládáním kostek (tetromino) na herní plochu (20 x 10) a zaplněním celých řad. Hra končí, když se další kostka již nevejde shora na herní plochu.

### 1) Hlavní menu

Po spuštění programu se objeví hlavní menu, na kterém lze kliknout na jedno z pěti tlačítek: 

- **1 Player** spustí hru Tetrisu s jedním hráčem.
- **2 Players** spustí hru dvou hráčů
- **Vs Computer** spustí hru proti počítači
- **Leaderboards** zobrazí žebříček nejlepších skóre
- **Quit** ukončí hru

### 2) Ovládání hry

Pro hraní Tetrisu je využito sedm různých tlačítek, při hře dvou hráčů pro každého jiné (druhý hráč uveden v závorce):

- **A (šipka doleva)** – posunutí kostky o 1 doleva
- **D (šipka doprava)** – posunutí kostky o 1 doprava
- **S (šipka dolů)** – posunutí kostky o 1 dolů (soft drop)
- **Space / W (šipka nahoru)** – posunutí kostky úplně dolů a položení (hard drop)
- **H (1 na numerické klávesnici)** – otočení kostky proti směru hodinových ručiček
- **J (2 na numerické klávesnici)** – otočení kostky po směru hodinových ručiček
- **K (3 na numerické klávesnici)** – odložení kostky stranou (do hold boxu) 

### 3)  Herní plocha

Při hraní Tetrisu má každý hráč svoji vlastní herní plochu s několika okolními elementy. Uprostřed obrazovky se nachází vlastní herní plocha, ve které hráč může pohybovat právě padající kostkou. Napravo od ní je možno vidět 3 následující kostky. Nalevo se nachází box, do kterého je možné odložit jednu kostku stranou (nelze udělat dvakrát po sobě). Nad herní plochou se zobrazuje dosažené skóre a nalevo od něj 3 statistiky: aktuální úroveň, combo a počet vyčištěných řad.

### 4) Skórování

Skóre je primárně udělováno za zaplnění řádků:

- 100 za 1 řádek
- 300 za 2 řádky
- 500 za 3 řádky
- 800 za 4 řádky

Pokud je však poslední položena kostka před dokončením řádky T-kostka (fialová), která byla do této pozice právě otočena, a má v aspoň třech ze 4 rohů plné poličko (T-Spin), tak se skóre zvedá na:

- 400 za 1 řádek
- 800 za 2 řádky
- 1200 za 3 řádky
- 1600 za 4 řádky

Tato skóre jsou ještě násobena jedním faktorem – aktuální úrovní. Ta se zvedá s celkovým počtem vyčištěných řádků a při vyšší úrovni padají kostky automaticky rychleji.

Skóre jde dále získat pomocí soft dropů (+1 skóre za 1 spadlé políčko) a hard dropů (+2 skóre za 1 spadlé políčko). Poslední způsob, jak získat skóre, je pomocí comba. To se zvedá, pokud několik kostek po sobě vždy vyčistí alespoň jeden řádek. Přičteno je pak pro každou položenou kostku jako (50 \* combo \* aktuální úroveň).

### 5) **Uložení nejvyššího skóre**

Hra si ukládá 5 nejvyšších zahraných skóre. Pokud je některé z nich překonáno ve hře jednoho hráče, tak se zobrazí okno pro zadání jména, pod kterým se skóre zaregistruje. Pokud je toto okno zavřeno, bude hráč uveden jako anonymní. Pro zobrazení nejvyšších skóre stačí na hlavním menu kliknout na tlačítko **Leaderboards**, které zobrazí 5 nejvyšších dosažených skóre. Dolním tlačítkem je pak možné se vrátit zpět na hlavní menu.

### 6) **Po konci hry**

Po konci hry se zobrazí překrytí, na kterém je možné vidět dosažené skóre. U hry dvou hráčů je pak skóre vítězného hráče zabarveno zeleně a druhé skóre červeně. Dále jsou na obrazovce dvě tlačítka:

- **Play Again** spustí novou hru v právě dohraném módu
- **Main Menu** nás vrátí zpátky na hlavní menu



