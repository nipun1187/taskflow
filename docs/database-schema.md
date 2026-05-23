# Database Schema

The application uses Entity Framework Core 8 against SQLite (dev) or SQL Server (prod).
ASP.NET Core Identity provides the user tables; the only domain table added is `Tasks`.

## ER Diagram

```
+----------------------+         +---------------------+
|   AspNetUsers        |  1   *  |       Tasks         |
|----------------------|---------|---------------------|
| Id           (PK)    |         | Id          (PK)    |
| UserName             |         | Title               |
| Email                |         | Description         |
| PasswordHash         |         | Status   (int)      |
| DisplayName          |         | Priority (int)      |
| CreatedAt            |         | DueDate             |
+----------------------+         | CreatedAt           |
                                 | CompletedAt         |
                                 | OwnerId    (FK)     |
                                 +---------------------+
```

## Tasks Table

| Column       | Type           | Constraints                         |
|--------------|----------------|-------------------------------------|
| Id           | INTEGER        | PK, identity                        |
| Title        | NVARCHAR(200)  | NOT NULL                            |
| Description  | NVARCHAR(2000) | NULL                                |
| Status       | INTEGER        | NOT NULL (enum: 0..3)               |
| Priority     | INTEGER        | NOT NULL (enum: 0..3)               |
| DueDate      | DATETIME       | NULL                                |
| CreatedAt    | DATETIME       | NOT NULL                            |
| CompletedAt  | DATETIME       | NULL                                |
| OwnerId      | NVARCHAR(450)  | NOT NULL, FK → AspNetUsers(Id)      |

Index: `(OwnerId, Status)` for fast filtered listing.

## Status / Priority Enums

```
Status:   0 Pending | 1 InProgress | 2 Completed | 3 Archived
Priority: 0 Low     | 1 Medium     | 2 High      | 3 Critical
```
