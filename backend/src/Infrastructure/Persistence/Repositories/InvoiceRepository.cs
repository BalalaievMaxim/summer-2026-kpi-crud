using GymManagement.Domain.Billing;
using GymManagement.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly GymManagementContext _context;

    public InvoiceRepository(GymManagementContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id)
    {
        var intId = GuidToInt(id);
        var entity = await _context.Invoices.FindAsync(intId);
        return entity is null ? null : InvoiceMapper.ToDomain(entity);
    }

    public async Task<List<Invoice>> GetByClientAsync(Guid clientId)
    {
        var intClientId = GuidToInt(clientId);
        var entities = await _context.Invoices
            .Where(i => i.ClientId == intClientId)
            .ToListAsync();

        return entities.Select(InvoiceMapper.ToDomain).ToList();
    }

    public async Task AddAsync(Invoice invoice)
    {
        var entity = InvoiceMapper.ToEntity(invoice);
        await _context.Invoices.AddAsync(entity);
    }

    public async Task UpdateAsync(Invoice invoice)
    {
        var entity = InvoiceMapper.ToEntity(invoice);
        _context.Invoices.Update(entity);
        await Task.CompletedTask;
    }

    private static int GuidToInt(Guid id)
    {
        var bytes = id.ToByteArray();
        return bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
    }
}
