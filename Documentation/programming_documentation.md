# Programátorská dokumentace ke hře Tetris

Cílem tohoto projektu bylo naprogramovat základní funkcionalitu Tetrisu s hrou jednoho nebo dvou hráčů, popř. hře proti počítači.

### Části programu

Program se skládá hlavně z těchto šesti souborů:

- **BlockQueue.cs**
- **Blocks.cs**
- **GameGrid.cs**
- **GameState.cs**
- **MainWindow.xaml(.cs)**
- **UserInput.xaml(.cs)**

Dále obsahuje složku **Assets**, ve které jsou sprity, pozadí a zvuky hry. A nakonec také soubor **HighScores.txt**, který obsahuje top 5 nejvyšších skóre dosažených hráčem ve formátu: skóre jméno\_hráče.

### 1) Blocks.cs

Obsahuje dvě hlavní třídy – **Position** a **Block** – a dále třídy pro každou kostku derivované od třídy Block.

Třída **Position** usnadňuje práci se souřadnicemi ve třídě Block. Obsahuje pouze proměnnou pro řádku a sloupec, které je pak možné využívat dále v kódu pro různé pozice kostek.

Třída **Block** obsahuje proměnné, které popisují aktuální natočení (rotationState) a pozici kostky (offset). Zahrnuje také startovací pozici (StartOffset) a pozice každé kostičky v rámci kostky. Společně s Id kostky (dle kterého lze pak kostky identifikovat a správně barvit herní plochu) jsou tyto proměnné overridnuty v každém typu kostky. Třída má dále několik metod pro rotace oběma směry (RotateCW a RotateCCW), pro pohyb kostky (Move) nebo pro resetování na výchozí pozici (Reset). Poslední metoda TilePositions vrací aktuální pozici každé kostičky (uvnitř kostky) na herní ploše, která se velmi hodí pro práci s kostkami na herní ploše.

Zbylé třídy pak už jen nastavují jednotlivé údaje pro každý typ kostičky v Tetrisu.

### 2) BlockQueue.cs

Obsahuje pouze jednu třídu **BlockQueue**, která má uložené tři následující kostky a pomocí metody RandomBlock umí náhodně generovat další. Tuto metodu využívá druhá metoda GetNextAndUpdate, která vygeneruje další kostku (různou od předcházející) a vrátí kostku na čele fronty.

### 3) GameGrid.cs

Obsahuje jednu třídu **GameGrid**, která si pamatuje herní plochu s id kostky v každém políčku. Definujeme v ní několik metod, které s herní plochou usnadňují práci, jako je IsInside, IsEmpty (jako vstup berou řádek a sloupec – tedy přímo jedno políčko), IsRowFull nebo IsRowEmpty. Dále pak metody pro čištění řádků – ClearRow, MoveRowDown a ClearFullRows. Všechny ostatní metody jsou použity při výpočtu hodnocení jednotlivých tahů počítače – DeepCopy (vytvoří kopii herní plochy, na které pak můžeme provádět změny bez ovlivnění stavu hry), HolesInColumn, HolesInBoard, ColumnHeight, ColsHeightDiff, WellsCount, NumberOfFullLines a NumberOfEmptyLines. Jejich význam je vysvětlen v sekci GameState.cs.

### 4) GameState.cs

Obsahuje znovu jednu větší třídu **GameState**, která v sobě drží stav celé hry (např. pro hru dvou hráčů existují dva objekty této třídy) a metody, které stav hry mění.

Uloženou má v sobě herní plochu (o dvě řady větší pro pokládání kostek a detekce konce hry), aktuální kostku, frontu následujících kostek, skóre, aktuální úroveň, combo, kostku drženou stranou a pár dalších informací o hře. Jednotlivé hry jsou při inicializaci odlišeny pomocí gameID (+ se při inicializaci udává, jestli hra už neskončila – je to pouze trik proti spuštění hry při zapnutí aplikace).

Zahrnuje v sobě metody na podržení kostky stranou (HoldBlock) a hýbání s kostkou, pokud to jde (RotateBlockCCW, RotateBlockCW, MoveBlockLeft, MoveBlockRight, MoveBlockDown, AutoMoveBlockDown a pomocná BlockFits). U rotací se také pokusíme s kostkou pohnout doprava/doleva o 1 (popř. o 2 u I kostky), neboť to velmi zlepší ovládání. Metoda IsGameOver pouze kontroluje, zda jsou nebo nejsou horní řady volné, a případně ukončí hru. Důležitá je metoda PlaceBlock, která nejen uloží kostku do herní plochy, ale také vyčistí úplné řady, spočítá přidané skóre (a detekuje T-Spiny), popř. ještě zvedne úroveň a combo, nebo ukončí hru / vezme další kostku z fronty. Poslední metody související s pohybem kostky řeší dropnutí, zjistí nejprve, jak moc dolů se kostka posune, a pak ji zde položí, připočtou skóre a pustí sound effect dropu.

Zbytek metod zde řeší pohyby computer hráče. Metoda MoveComputer je volaná z loopu v MainWindow.xaml.cs a ta se pokusí zjistit nejlepší tah, ať už je to položení kostky nebo výměnu za kostku, která je daná stranou, a poté ho vykonat. Vyhodnocení nejlepšího tahu zajistí metoda FindBestMove, která s aktuální kostkou vyzkouší všechny možné pozice a orientace položení a pro každou z nich vypočte hodnocení tahu. To jsem se rozhodl zjišťovat z 6 různých faktorů:

- **Počet prázdných řádků** – chceme mít co nejvíce prázdných políček a nezvyšovat aktuální výšku postavených kostek o moc
- **Počet plných řádků** – řádky které zmizí po aplikování tahu a přidají skóre,  takže ty rozhodně chceme
- **Délka pádu** – větší je lepší, aby se zaplnily větší prohlubně
- **Počet děr** – díra = prázdné políčko s plným políčkem někde výše; dobré snažit se minimalizovat
- **Počet prohlubin** – prohlubina = jeden sloupec, který je alespoň o 3 menší než okolní; pokud se snažíme snížit počet děr, tak počítač začne stavět dlouhé sloupce, které ukončí hru; tomu se toto snaží zabránit
- **Celková suma rozdílů výšek** – znovu dobře funguje na zmenšování výšek a dokončování řad

Tyto faktory jsou pak přenásobeny vahami, které jdou snadno modifikovat pro změnu obtížnosti počítačového hráče.

### 5) MainWindow.xaml(.cs)

Soubor **MainWindow.xaml** obsahuje rozložení hlavní obrazovky a elementů na ní. Rozděluje obrazovku do 2 řádků a 6 sloupců, což nám dobře stačí na vytvoření dvou herních ploch vedle sebe s okolními elementy. A při hře jednoho hráče se pouze jednoduše 3 sloupce skryjí. Herní plochy jsou vždy v pozadí načtené, různá menu je pouze překrývají. Kromě obrazovkou s herními plochami tedy zde máme ještě překrytí po konci hry, pro hlavní menu a pro žebříček nejlepšího skóre.

Druhý soubor **MainWindow.xaml.cs** slouží k ovládání UI a obsahuje jednu třídu **MainWindow**. Ta v sobě má uložené stavy her, zobrazování jednotlivých dílků, kostek a celé herní plochy, hudbu v pozadí nebo zvolený herní mód. Metoda SetupGameCanvas nastaví vykreslování a ovládání herní plochy v UI. Plochu i s ostatními elementy okolo pak vykresluje metoda Draw, která používá několik různých draw- metod.

Nejdůležitější metoda je zde GameLoop, která je zavolaná při spuštění hry, spustí hudbu v pozadí (a nastaví, aby se loopovala) a poté dokud hra neskončí volá metodu Draw a také posouvá kostku automaticky dolů o 1. Pokud je pak herní mód nastavený na hru s počítačem, pak také zjišťuje jeho další tah. Po konci hry zobrazí game over menu s finálním skóre. Pokud také zjistí, že je skóre vyšší než některé z high skóre, tak otevře nové okno pro zadání uživatelského jména (**UserInput.xaml**) a poté skóre zapíše do souboru **HighScores.txt**.

Ovládání hry řeší metody HandleKeyPressesGame1/2, které jsou volané z metody Window\_KeyDown, která se automaticky aktivuje při stisknutí libovolné klávesy. Dále následují metody pro kliknutí na jednotlivá tlačítka v menu. Některá z nich pouze jednoduše přepnou zobrazovanou scénu, jiné zahájí novou hru zavoláním metody GameLoop na nový GameState a počkáním na doběhnutí.

### 6) UserInput.xaml(.cs)

Tyto poslední dva soubory pouze zobrazují jednoduché okno pro zadání uživatelského jména s tlačítkem OK. Menší změna nastane pouze tehdy, když uživatel okno zavře místo zadání jména – pak je do žebříčku zapsán jako anonymní uživatel.
