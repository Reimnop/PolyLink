using Microsoft.EntityFrameworkCore;
using PolyLink.Common.Model;

namespace PolyLink.Server.Controller;

public class PolyLinkContext(DbContextOptions<PolyLinkContext> options) : DbContext(options)
{
    public required DbSet<Profile> Profiles { get; set; }
}