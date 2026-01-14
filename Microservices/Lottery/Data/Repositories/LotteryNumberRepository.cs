using CryptoJackpot.Lottery.Data.Context;
using CryptoJackpot.Lottery.Domain.Enums;
using CryptoJackpot.Lottery.Domain.Exceptions;
using CryptoJackpot.Lottery.Domain.Interfaces;
using CryptoJackpot.Lottery.Domain.Models;
using Microsoft.EntityFrameworkCore;
namespace CryptoJackpot.Lottery.Data.Repositories;

public class LotteryNumberRepository : ILotteryNumberRepository
{
    private readonly LotteryDbContext _context;
    private static readonly Random _random = new();

    public LotteryNumberRepository(LotteryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LotteryNumber>> GetNumbersByLotteryAsync(Guid lotteryId)
        => await _context.LotteryNumbers.Where(x => x.LotteryId == lotteryId).ToListAsync();

    /// <summary>
    /// Obtiene solo los números vendidos (más eficiente para exclusión)
    /// </summary>
    public async Task<HashSet<int>> GetSoldNumbersAsync(Guid lotteryId)
        => (await _context.LotteryNumbers
                .Where(x => x.LotteryId == lotteryId && !x.IsAvailable)
                .Select(x => x.Number)
                .ToListAsync())
            .ToHashSet();

    /// <summary>
    /// Verifica si un número específico está disponible (O(log N) con índice compuesto)
    /// </summary>
    public async Task<bool> IsNumberAvailableAsync(Guid lotteryId, int number, int series)
        => !await _context.LotteryNumbers
            .AnyAsync(x => x.LotteryId == lotteryId && x.Number == number && x.Series == series);

    /// <summary>
    /// Verifica disponibilidad de múltiples números en una sola consulta (elimina problema N+1)
    /// Usa el índice IX_LotteryNumbers_LotteryId_Number_Series para búsqueda O(log N) por número
    /// </summary>
    public async Task<List<int>> GetAlreadyReservedNumbersAsync(Guid lotteryId, int series, IEnumerable<int> numbers)
    {
        var numbersList = numbers.ToList();
        
        return await _context.LotteryNumbers
            .Where(x => x.LotteryId == lotteryId && 
                        x.Series == series && 
                        numbersList.Contains(x.Number))
            .Select(x => x.Number)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene N números aleatorios disponibles (sin considerar series específicas).
    /// Útil para sugerir números al usuario antes de seleccionar serie.
    /// </summary>
    public async Task<List<int>> GetRandomAvailableNumbersAsync(Guid lotteryId, int count, int maxNumber, int minNumber = 1)
    {
        // Optimización: Solo traer los números vendidos, no todos los registros
        var soldNumbers = await _context.LotteryNumbers
            .Where(x => x.LotteryId == lotteryId)
            .Select(x => x.Number)
            .Distinct()
            .ToListAsync();

        var soldSet = soldNumbers.ToHashSet();
        var totalRange = maxNumber - minNumber + 1;

        // Si hay pocos números vendidos, generar disponibles es más eficiente
        if (soldSet.Count < totalRange * 0.7) // Menos del 70% vendido
        {
            return Enumerable.Range(minNumber, totalRange)
                .Where(n => !soldSet.Contains(n))
                .OrderBy(_ => _random.Next())
                .Take(count)
                .ToList();
        }

        // Si hay muchos vendidos, usar sampling aleatorio con verificación
        var available = new List<int>(count);
        var attempts = 0;
        var maxAttempts = count * 10;

        while (available.Count < count && attempts < maxAttempts)
        {
            var candidate = _random.Next(minNumber, maxNumber + 1);
            if (!soldSet.Contains(candidate) && !available.Contains(candidate))
            {
                available.Add(candidate);
            }
            attempts++;
        }

        return available;
    }

    /// <summary>
    /// Obtiene N combinaciones (número, serie) aleatorias disponibles.
    /// Optimizado para loterías con millones de combinaciones.
    /// </summary>
    public async Task<List<(int Number, int Series)>> GetRandomAvailableNumbersWithSeriesAsync(
        Guid lotteryId, int count, int maxNumber, int totalSeries, int minNumber = 1)
    {
        // Estrategia: Para loterías grandes, usar sampling aleatorio con verificación en lote
        // En lugar de cargar todos los vendidos, verificamos candidatos aleatorios
        
        var totalRange = maxNumber - minNumber + 1;
        var totalCombinations = (long)totalRange * totalSeries;
        
        // Obtener cantidad de números vendidos para decidir estrategia
        var soldCount = await _context.LotteryNumbers
            .Where(x => x.LotteryId == lotteryId)
            .CountAsync();

        var percentageSold = (double)soldCount / totalCombinations;

        // Si menos del 50% está vendido, usar sampling aleatorio
        if (percentageSold < 0.5)
        {
            return await GetRandomBySamplingAsync(lotteryId, count, maxNumber, totalSeries, minNumber);
        }

        // Si más del 50% vendido, cargar vendidos y generar disponibles
        return await GetRandomFromAvailablePoolAsync(lotteryId, count, maxNumber, totalSeries, minNumber);
    }

    private async Task<List<(int Number, int Series)>> GetRandomBySamplingAsync(
        Guid lotteryId, int count, int maxNumber, int totalSeries, int minNumber)
    {
        var result = new List<(int Number, int Series)>(count);
        var maxAttempts = count * 20; // Margen amplio para colisiones
        var attempts = 0;
        var batchSize = Math.Min(count * 3, 100); // Verificar en lotes

        var candidates = new HashSet<(int, int)>();

        while (result.Count < count && attempts < maxAttempts)
        {
            // Generar lote de candidatos
            while (candidates.Count < batchSize && attempts < maxAttempts)
            {
                var number = _random.Next(minNumber, maxNumber + 1);
                var series = _random.Next(1, totalSeries + 1);
                candidates.Add((number, series));
                attempts++;
            }

            if (!candidates.Any()) break;

            // Verificar disponibilidad en lote (una sola consulta)
            var candidatesList = candidates.ToList();
            var reservedInBatch = await _context.LotteryNumbers
                .Where(x => x.LotteryId == lotteryId)
                .Where(x => candidatesList.Any(c => c.Item1 == x.Number && c.Item2 == x.Series))
                .Select(x => new { x.Number, x.Series })
                .ToListAsync();

            var reservedSet = reservedInBatch.Select(r => (r.Number, r.Series)).ToHashSet();

            foreach (var candidate in candidatesList)
            {
                if (!reservedSet.Contains(candidate) && result.Count < count)
                {
                    result.Add(candidate);
                }
            }

            candidates.Clear();
        }

        return result;
    }

    private async Task<List<(int Number, int Series)>> GetRandomFromAvailablePoolAsync(
        Guid lotteryId, int count, int maxNumber, int totalSeries, int minNumber)
    {
        // Cuando hay muchos vendidos, es mejor obtener los vendidos y excluirlos
        var soldCombinations = await _context.LotteryNumbers
            .Where(x => x.LotteryId == lotteryId)
            .Select(x => new { x.Number, x.Series })
            .ToListAsync();

        var soldSet = soldCombinations.Select(s => (s.Number, s.Series)).ToHashSet();

        // Generar combinaciones disponibles (esto puede ser costoso para loterías muy grandes)
        var available = new List<(int Number, int Series)>();

        // Usar un generador lazy para no crear toda la lista en memoria
        for (var series = 1; series <= totalSeries && available.Count < count * 10; series++)
        {
            for (var number = minNumber; number <= maxNumber && available.Count < count * 10; number++)
            {
                if (!soldSet.Contains((number, series)))
                {
                    available.Add((number, series));
                }
            }
        }

        // Mezclar y tomar los necesarios
        return available
            .OrderBy(_ => _random.Next())
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Agrega múltiples números de lotería.
    /// Lanza DuplicateNumberReservationException si hay conflicto de concurrencia.
    /// </summary>
    public async Task AddRangeAsync(IEnumerable<LotteryNumber> lotteryNumbers)
    {
        var numbers = lotteryNumbers.ToList();
        await _context.LotteryNumbers.AddRangeAsync(numbers);
        
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Extraer información del contexto para la excepción de dominio
            var firstNumber = numbers.FirstOrDefault();
            var lotteryId = firstNumber?.LotteryId ?? Guid.Empty;
            var ticketId = firstNumber?.TicketId ?? Guid.Empty;
            
            throw new DuplicateNumberReservationException(lotteryId, ticketId, ex);
        }
    }

    /// <summary>
    /// Detecta si la excepción es por violación de índice único (concurrencia)
    /// Soporta PostgreSQL, SQL Server y SQLite
    /// </summary>
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var message = ex.Message + (ex.InnerException?.Message ?? string.Empty);
        return message.Contains("23505") || // PostgreSQL unique_violation
               message.Contains("2601") ||  // SQL Server duplicate key in unique index
               message.Contains("2627") ||  // SQL Server duplicate key in primary key/unique constraint
               message.Contains("UNIQUE constraint", StringComparison.OrdinalIgnoreCase) || // SQLite
               message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("IX_LotteryNumbers_LotteryId_Number_Series", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Libera los números asociados a un ticket (elimina los registros)
    /// </summary>
    public async Task<bool> ReleaseNumbersByTicketAsync(Guid ticketId)
    {
        var numbers = await _context.LotteryNumbers
            .Where(x => x.TicketId == ticketId)
            .ToListAsync();

        if (!numbers.Any())
            return false;

        _context.LotteryNumbers.RemoveRange(numbers);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Reserves lottery numbers for an order (during checkout)
    /// </summary>
    public async Task<bool> ReserveNumbersAsync(List<Guid> numberIds, Guid orderId)
    {
        var numbers = await _context.LotteryNumbers
            .Where(x => numberIds.Contains(x.Id) && x.Status == NumberStatus.Available)
            .ToListAsync();

        if (numbers.Count != numberIds.Count)
            return false; // Some numbers are not available

        var now = DateTime.UtcNow;
        foreach (var number in numbers)
        {
            number.Status = NumberStatus.Reserved;
            number.OrderId = orderId;
            number.ReservationExpiresAt = now.AddMinutes(5);
            number.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Confirms lottery numbers as sold (after payment)
    /// </summary>
    public async Task<bool> ConfirmNumbersSoldAsync(List<Guid> numberIds, Guid ticketId)
    {
        var numbers = await _context.LotteryNumbers
            .Where(x => numberIds.Contains(x.Id) && x.Status == NumberStatus.Reserved)
            .ToListAsync();

        if (numbers.Count != numberIds.Count)
            return false;

        var now = DateTime.UtcNow;
        foreach (var number in numbers)
        {
            number.Status = NumberStatus.Sold;
            number.TicketId = ticketId;
            number.ReservationExpiresAt = null;
            number.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Releases reserved numbers back to available (order expired/cancelled)
    /// </summary>
    public async Task<bool> ReleaseNumbersByOrderAsync(Guid orderId)
    {
        var numbers = await _context.LotteryNumbers
            .Where(x => x.OrderId == orderId && x.Status == NumberStatus.Reserved)
            .ToListAsync();

        if (!numbers.Any())
            return false;

        var now = DateTime.UtcNow;
        foreach (var number in numbers)
        {
            number.Status = NumberStatus.Available;
            number.OrderId = null;
            number.ReservationExpiresAt = null;
            number.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Gets lottery numbers by their IDs
    /// </summary>
    public async Task<List<LotteryNumber>> GetByIdsAsync(List<Guid> numberIds)
        => await _context.LotteryNumbers
            .Where(x => numberIds.Contains(x.Id))
            .ToListAsync();

    /// <summary>
    /// Finds an available number (first available series if series not specified)
    /// </summary>
    public async Task<LotteryNumber?> FindAvailableNumberAsync(Guid lotteryId, int number, int? series = null)
    {
        var query = _context.LotteryNumbers
            .Where(x => x.LotteryId == lotteryId && 
                        x.Number == number && 
                        x.Status == NumberStatus.Available);

        if (series.HasValue)
        {
            query = query.Where(x => x.Series == series.Value);
        }

        return await query
            .OrderBy(x => x.Series)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Finds available numbers matching the specified numbers and series
    /// </summary>
    public async Task<List<LotteryNumber>> FindAvailableNumbersAsync(
        Guid lotteryId, 
        int series, 
        IEnumerable<int> numbers)
    {
        var numbersList = numbers.ToList();
        
        return await _context.LotteryNumbers
            .Where(x => x.LotteryId == lotteryId && 
                        x.Series == series &&
                        numbersList.Contains(x.Number) &&
                        x.Status == NumberStatus.Available)
            .ToListAsync();
    }

    /// <summary>
    /// Updates a single lottery number
    /// </summary>
    public async Task<LotteryNumber> UpdateAsync(LotteryNumber lotteryNumber)
    {
        lotteryNumber.UpdatedAt = DateTime.UtcNow;
        _context.LotteryNumbers.Update(lotteryNumber);
        await _context.SaveChangesAsync();
        return lotteryNumber;
    }

    /// <summary>
    /// Updates multiple lottery numbers
    /// </summary>
    public async Task UpdateRangeAsync(IEnumerable<LotteryNumber> lotteryNumbers)
    {
        var now = DateTime.UtcNow;
        var enumerable = lotteryNumbers.ToList();
        foreach (var number in enumerable)
        {
            number.UpdatedAt = now;
        }
        
        _context.LotteryNumbers.UpdateRange(enumerable);
        await _context.SaveChangesAsync();
    }
}