# AI Agent System

Aplikace pro správu požadavků na tvorbu custom AI agentů s databázovým úložištěm a REST API.

---

## Základní popis

Aplikace umožňuje:

* zákazníkovi vyplnit formulář a odeslat požadavek na vytvoření AI agenta,
* administrátorovi převzít požadavek, zpracovat jej nebo stornovat,
* import dat do databáze,
* správu celého procesu pomocí stavů a transakcí.

Projekt je vytvořen jako **školní projekt** a slouží k demonstraci práce s databází, REST API a jednoduchým frontendem.

---

## Rychlé spuštění

### Požadavky

* Windows / Linux / macOS
* .NET SDK 7.0+
* Microsoft SQL Server (LocalDB, Express, školní server)
* Git
  
## Konfigurace databáze (`appsettings.json`)
Upravte soubor appsettings.json a vyplňte údaje o databázi.
```json
{
  "Database": {
    "DataSource": ".\\SQLEXPRESS",
    "Name": "",
    "User": "",
    "Password": ""
  }
}
```

### Popis polí

* `DataSource` – adresa SQL Serveru (Pro Windows autentizaci nechte .\\SQLEXPRESS)
* `Name` – název databáze
* `User`, `Password` – vyplnit pouze při SQL autentizaci

---

## Vytvoření databáze

Pokud databáze neexistuje, je nutné ji vytvořit ručně:

```sql
CREATE DATABASE databaze;
```

Poté aplikace při běhu pracuje s existující strukturou tabulek.
### Postup spuštění backendu
Otevřte příkazový řádek a dostaňte se do složky s projektem.
```bash
git clone [repository-url]
cd backend
dotnet restore
dotnet build
dotnet run
```

Po úspěšném spuštění běží REST API standardně na adrese:

```
http://localhost:8080
```

### Spuštění frontendu

Frontend je čistý HTML/CSS/JavaScript a **není potřeba build**:

* otevřete `index.html` v prohlížeči
* nebo použijte jednoduchý server (např. Live Server ve VS Code)

---

## Import dat (CSV)

Aplikace umožňuje import dat přes frontend.

### Podporované tabulky

* `Contact`
* `Administrator`

### Formát CSV

**Contact:**

```csv
jmeno,prijmeni,email
Jan,Novák,jan.novak@email.cz
```

**Administrator:**

```csv
jmeno,prijmeni,email
Admin,Test,admin@email.cz
```

* Oddělovač: čárka `,`
* Maximální velikost souboru: 5 MB
* Volitelná hlavička (lze zaškrtnout ve frontend UI)

---

## Hlavní funkce aplikace

1. **Odeslání požadavku**

   * zákazník vyplní jméno, příjmení, email a popis požadavku
   * vytvoří se nový záznam v databázi

2. **Přiřazení admina**

   * admin převezme požadavek
   * změna stavu z `Nový` → `Řeší se`

3. **Zpracování požadavku**

   * admin požadavek dokončí (vloží odpověď)
   * změna stavu na `Uzavřený`

4. **Storno požadavku**

   * admin může požadavek zrušit
   * změna stavu na `Storno`

5. **Import CSV**

   * CSV import přes frontend

---

## Stavy požadavku

* `Nový`
* `Řeší se`
* `Uzavřený`
* `Storno`

Změny stavů jsou kontrolovány validační logikou na backendu.

---

## Validace vstupů

* **Email:** povinný, validní formát
* **Text požadavku:** nesmí být prázdný
* **CSV soubory:**

  * pouze `.csv`
  * max. velikost 5 MB

---

## Řešení problémů

### API neběží

* Zkontrolujte, zda je backend spuštěn
* Ověřte dostupnost `http://localhost:8080`

### Nelze se připojit k databázi

* Je SQL Server spuštěn?
* Existuje databáze?
* Odpovídá konfigurace v `appsettings.json`?

### Import selhal

* Zkontrolujte formát CSV
* Ověřte, že jsou vyplněna všechna povinná pole

---

## Použité technologie

* C# (.NET)
* Microsoft SQL Server
* REST API
* HTML, CSS, JavaScript (Vanilla JS)

---

Autor: David Tesař
