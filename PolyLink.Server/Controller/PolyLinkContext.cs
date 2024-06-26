using Microsoft.EntityFrameworkCore;
using PolyLink.Server.Model;

namespace PolyLink.Server.Controller;

public class PolyLinkContext(DbContextOptions<PolyLinkContext> options) : DbContext(options)
{
    public required DbSet<Session> Sessions { get; set; }
}