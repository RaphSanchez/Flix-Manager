using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorMovies.Server.DataStore.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataDependent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /// Inserts data to dbo.GenreMovie linking table.
            migrationBuilder.Sql(@"
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (2, 1)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (4, 1)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (5, 1)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1017, 1)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (4, 2)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (5, 2)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1017, 2)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1018, 2)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1019, 2)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (4, 3)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (5, 3)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1016, 3)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1017, 3)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (2, 4)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1014, 4)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1, 1021)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (2, 1021)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1014, 1021)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1, 1022)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1015, 1022)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1016, 1022)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (2, 1033)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1018, 1033)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (4, 1035)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (5, 1035)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1017, 1035)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1018, 1035)

");

            /// Inserts data to dbo.MovieCharacters linking table.
            migrationBuilder.Sql(@"
SET IDENTITY_INSERT [dbo].[MovieCharacters] ON
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (103, N'Mal', 1, 20, 3, N'admin@email.com', N'2023-03-24 13:19:20', 0, N'admin@email.com', N'2023-03-24 13:19:20')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (104, N'Phillipa', 2, 40, 3, N'admin@email.com', N'2023-03-24 13:19:20', 0, N'admin@email.com', N'2023-03-24 13:19:20')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (105, N'Sara', 1, 19, 4, N'admin@email.com', N'2023-03-24 13:21:47', 0, N'admin@email.com', N'2023-03-24 13:21:47')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (106, N'Halley', 2, 15, 4, N'admin@email.com', N'2023-03-24 13:21:47', 0, N'admin@email.com', N'2023-03-24 13:21:47')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (109, N'Diana', 1, 18, 2, N'admin@email.com', N'2023-03-24 13:24:35', 0, N'admin@email.com', N'2023-03-24 13:24:35')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (110, N'Hippolyta', 2, 17, 2, N'admin@email.com', N'2023-03-24 13:24:35', 0, N'admin@email.com', N'2023-03-24 13:24:35')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (125, N'Laura', 1, 7, 1022, N'admin@email.com', N'2023-03-27 08:59:28', 0, N'admin@email.com', N'2023-03-27 08:59:28')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (126, N'Malkina', 2, 16, 1022, N'admin@email.com', N'2023-03-27 08:59:28', 0, N'admin@email.com', N'2023-03-27 08:59:28')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (138, N'The Devil', 1, 14, 1033, N'admin@email.com', N'2023-03-27 11:19:30', 0, N'admin@email.com', N'2023-03-27 11:19:30')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (139, N'Allison', 2, 43, 1033, N'admin@email.com', N'2023-03-27 11:19:30', 0, N'admin@email.com', N'2023-03-27 11:19:30')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (140, N'Barbara', 1, 2, 1021, N'admin@email.com', N'2023-03-27 11:21:20', 0, N'admin@email.com', N'2023-03-27 11:21:20')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (141, N'Sequins', 2, 22, 1021, N'admin@email.com', N'2023-03-27 11:21:20', 0, N'admin@email.com', N'2023-03-27 11:21:20')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (142, N'May', 1, 21, 1, N'admin@email.com', N'2023-03-27 11:21:43', 0, N'admin@email.com', N'2023-03-27 11:21:43')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (143, N'Maria', 2, 42, 1, N'admin@email.com', N'2023-03-27 11:21:43', 0, N'admin@email.com', N'2023-03-27 11:21:43')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (145, N'Bubble', 1, 44, 1035, N'admin@email.com', N'2023-04-06 11:15:21', 0, N'admin@email.com', N'2023-04-06 11:15:21')
SET IDENTITY_INSERT [dbo].[MovieCharacters] OFF
");

            /// Inserts data to dbo.MoviePerson linking table.
            migrationBuilder.Sql(@"
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (21, 1)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (42, 1)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (17, 2)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (18, 2)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (20, 3)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (40, 3)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (15, 4)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (19, 4)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (2, 1021)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (22, 1021)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (7, 1022)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (16, 1022)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (14, 1033)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (43, 1033)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (44, 1035)

");

            /// Inserts data to dbo.AspNetUserClaims dependent table.
            migrationBuilder.Sql(@"
SET IDENTITY_INSERT [dbo].[AspNetUserClaims] ON
INSERT INTO [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (72, N'728f324a-81f1-4c07-a42f-21d13a07af4c', N'content.creator', N'creator')
INSERT INTO [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (73, N'728f324a-81f1-4c07-a42f-21d13a07af4c', N'user.reader', N'reader')
INSERT INTO [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (74, N'728f324a-81f1-4c07-a42f-21d13a07af4c', N'content.editor', N'editor')
INSERT INTO [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (75, N'728f324a-81f1-4c07-a42f-21d13a07af4c', N'content.cleaner', N'cleaner')
INSERT INTO [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (76, N'728f324a-81f1-4c07-a42f-21d13a07af4c', N'user.creator', N'creator')
INSERT INTO [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (77, N'728f324a-81f1-4c07-a42f-21d13a07af4c', N'user.editor', N'editor')
INSERT INTO [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (78, N'728f324a-81f1-4c07-a42f-21d13a07af4c', N'user.cleaner', N'cleaner')
INSERT INTO [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (79, N'64b832c1-7a73-4905-8a2b-0d22eff6d557', N'content.creator', N'creator')
INSERT INTO [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (80, N'64b832c1-7a73-4905-8a2b-0d22eff6d557', N'user.creator', N'creator')
SET IDENTITY_INSERT [dbo].[AspNetUserClaims] OFF
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}


