# PitStop 🚗💨

PitStop este o platformă web cuprinzătoare pentru directorul auto din România. Aceasta servește ca punct unic de informare pentru utilizatorii care doresc să găsească și să evalueze diverse servicii auto în toată România, inclusiv service-uri auto, magazine de piese, spălătorii auto, vulcanizări și multe altele.

## 🌟 Funcționalități

### 🔍 Căutare și Descoperire
- Căutare după cuvinte cheie, oraș sau județ.
- Filtrare după categorie, rating minim și disponibilitate curentă ("Deschis acum").
- Sortare după recomandări, rating sau numărul de recenzii.
- Carduri de prezentare detaliate pentru unități, afișând fotografii, ratinguri și status.

### 🏪 Profiluri de Servicii Auto
- Pagini de profil complete cu galerii foto și lightbox.
- Liste detaliate de servicii cu intervale de preț.
- Program de funcționare și informații de contact (Telefon, Email, WhatsApp).
- Integrare interactivă cu Google Maps pentru indicații de orientare.
- Recenzii ale clienților și detalierea ratingului.

### 👤 Conturi Utilizator
- Înregistrare și autentificare securizată (ASP.NET Core Identity).
- Suport pentru Google OAuth pentru acces facil.
- Listă personalizată de "Favorite" pentru a salva unitățile preferate.
- Gestionarea recenziilor (scrie, editează sau șterge propriile recenzii).

### 🏗️ Înrolare și Gestionare Unități
- Formular public de cerere pentru proprietarii de unități auto care doresc să își listeze serviciile.
- Flux de lucru pentru aprobarea de către admin, cu notificări automate.
- **Panou de Control pentru Proprietari** dedicat pentru gestionarea profilului, încărcarea fotografiilor și actualizarea serviciilor.
- **Panou de Control Admin** pentru supravegherea tuturor cererilor, utilizatorilor și listărilor.

## 🛠️ Tehnologii Utilizate

- **Framework:** .NET 10
- **UI:** Blazor Server (modul Interactive Server)
- **Styling:** Tailwind CSS v4
- **Bază de date:** PostgreSQL (Producție), SQLite (Fallback pentru dezvoltare)
- **ORM:** Entity Framework Core 10
- **Autentificare:** ASP.NET Core Identity
- **Pictograme:** Google Material Symbols
- **Fonturi:** Manrope (titluri), Inter (corp text)

## 🚀 Primii Pași

### Cerințe prealabile
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js & npm](https://nodejs.org/) (pentru Tailwind CSS)
- PostgreSQL (opțional, implicit folosește SQLite pentru dezvoltare)

### Instalare

1. **Clonează depozitul:**
   ```bash
   git clone https://github.com/your-repo/pitstop.git
   cd pitstop
   ```

2. **Instalează dependențele:**
   ```bash
   cd PitStop.Web
   npm install
   cd ..
   ```

3. **Configurarea Bazei de Date:**
   În mod implicit, proiectul folosește SQLite în mediul de dezvoltare. Dacă dorești să folosești PostgreSQL, actualizează `appsettings.Development.json` cu șirul tău de conexiune și asigură-te că `DatabaseProvider` nu este setat pe `"Sqlite"`.

4. **Rulează aplicația:**
   ```bash
   dotnet run --project PitStop.Web
   ```

## 📂 Structura Proiectului

Proiectul urmează principiile Clean Architecture:

- **`PitStop.Domain`**: Entități de bază, enumerări și logică de domeniu.
- **`PitStop.Application`**: Logică de business, interfețe și DTO-uri.
- **`PitStop.Infrastructure`**: Acces la date (EF Core), implementarea Identity și servicii externe.
- **`PitStop.Web`**: Interfața utilizator Blazor Server, componente și active statice.

## 💻 Comenzi Comune

| Sarcina | Comanda |
|---|---|
| Rulare Proiect | `dotnet run --project PitStop.Web` |
| Mod Watch (hot reload) | `dotnet watch --project PitStop.Web` |
| Watcher Tailwind CSS | `cd PitStop.Web && npm run build:css` |
| Build Soluție | `dotnet build PitStop.sln` |
| Adăugare Migrare | `dotnet ef migrations add <Nume> --project PitStop.Infrastructure --startup-project PitStop.Web` |
| Actualizare Bază de Date | `dotnet ef database update --project PitStop.Infrastructure --startup-project PitStop.Web` |

> **Hot reload în Rider:** Rulează proiectul normal și folosește iconița flacără din toolbar. Pentru modificări CSS, rulează și `npm run build:css` într-un terminal — Tailwind este compilat separat și nu este acoperit de hot reload-ul Rider.

## 🎨 UI & Design

Interfața este construită cu Tailwind CSS v4 folosind un sistem de design personalizat.
- **Culoare Principală:** Brand Red (`#C0392B`)
- **Tipografie:** Manrope pentru titluri, Inter pentru text.

---
*PitStop - Găsește serviciul auto potrivit, rapid și ușor.*
