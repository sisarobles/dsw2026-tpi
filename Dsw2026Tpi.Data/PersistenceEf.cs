using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Data.Sqlite;

namespace Dsw2026Tpi.Data;

public class PersistenceEf: IPersistence
{
    private readonly Dsw2026TpiDbContext _context;
    private IDbContextTransaction? _transaction;

    public PersistenceEf(Dsw2026TpiDbContext context)
    {
        _context = context;
    }


    public async Task<T> Add<T>(T entity) where T : EntityBase
    {
        await _context.AddAsync(entity);

        try
        {
            await _context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
            when (entity is Appointment &&
              ex.InnerException is SqliteException sqliteException &&
              sqliteException.SqliteErrorCode == 19 &&
              sqliteException.SqliteExtendedErrorCode == 2067 &&
              sqliteException.Message.Contains(
                "Appointments.AvailabilitySlotId",
               StringComparison.OrdinalIgnoreCase))
            {
                throw new Dsw2026Tpi.CrossCutting.Exceptions.ConflictException(
                    Dsw2026Tpi.CrossCutting.Resources.ErrorCodes.APPOINTMENT_CONFLICT,
                    nameof(Dsw2026Tpi.CrossCutting.Resources.ErrorCodes.APPOINTMENT_CONFLICT));
            }
    }

    public async Task<T> Delete<T>(T entity) where T : EntityBase
    {
        var a = entity.Id;
        _context.Remove(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T?> First<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include).FirstOrDefaultAsync(predicate);
    }

    public async Task<IEnumerable<T>?> GetAll<T>(params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include).ToListAsync();
    }

    public async Task<T?> GetById<T>(Guid id, params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include).FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<T>?> GetFiltered<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include).Where(predicate).ToListAsync();
    }

    public async Task<T> Update<T>(T entity) where T : EntityBase
    {
        _context.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Pagination<T>> Paginate<T, TKey>(int pageSize, int pageIndex, Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> sortOrder, params string[] includes) where T : EntityBase
    {
        pageSize = Math.Abs(pageSize);
        pageIndex = Math.Abs(pageIndex) == 0 ? 0 : Math.Abs(pageIndex) - 1;

        var filtered = Include(_context.Set<T>(), includes)
                 .Where(predicate)
                 .OrderBy(sortOrder);

        var total = await filtered.CountAsync();

        
        async Task<Pagination<T>> GetPage(int skip, int take)
        {
            var data = await filtered.Skip(skip)
                    .Take(take)
                    .ToListAsync();

            return new Pagination<T>(pageSize, pageIndex, data, total);
        }
        
        if (total > pageSize * pageIndex)
        {
            return await GetPage(pageIndex * pageSize, pageSize);
        }

        if (total < pageSize)
        {
            return new Pagination<T>(pageSize, pageIndex, await filtered.ToListAsync(), total);
        }

        var targetPageIndex = pageIndex - 1;

        while (true)
        {
            if (total > targetPageIndex * pageSize)
            {
                return await GetPage(targetPageIndex * pageSize, pageSize);
            }

            targetPageIndex--;

            if (targetPageIndex < 0) return new Pagination<T>(pageSize, 0,[], 0);
        }
    }

    private static IQueryable<T> Include<T>(IQueryable<T> query, string[] includes) where T : EntityBase
    {
        var includedQuery = query;

        foreach (var include in includes)
        {
            includedQuery = includedQuery.Include(include);
        }
        return includedQuery;
    }


    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException(
                "Ya existe una transacción activa.");
        }

        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException(
                "No existe una transacción activa para confirmar.");
        }

        await _transaction.CommitAsync();

        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction == null)
        {
            return;
        }

        var transaction = _transaction;

        try
        {
            await transaction.RollbackAsync();
        }
        finally
        {
            await transaction.DisposeAsync();
            _transaction = null;
        }
    }

}
