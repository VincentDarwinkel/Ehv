﻿using Authentication_Service.Dal.Interface;
using Authentication_Service.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication_Service.Dal
{
    public class PendingLoginDal : IPendingLoginDal
    {
        private readonly DataContext _context;

        public PendingLoginDal(DataContext context)
        {
            _context = context;
        }

        public async Task Add(PendingLoginDto pendingLogin)
        {
            await _context.PendingLogin.AddAsync(pendingLogin);
            await _context.SaveChangesAsync();
        }

        public async Task<PendingLoginDto> Find(PendingLoginDto pendingLogin)
        {
            return await _context.PendingLogin
                .FirstOrDefaultAsync(pl => pl.UserUuid == pendingLogin.UserUuid &&
                                           pl.AccessCode == pendingLogin.AccessCode);
        }

        public async Task Remove(Guid userUuid)
        {
            PendingLoginDto pendingLoginToRemove =
                await _context.PendingLogin
                    .FirstOrDefaultAsync(pl => pl.UserUuid == userUuid);

            if (pendingLoginToRemove != null)
            {
                _context.PendingLogin.Remove(pendingLoginToRemove);
                await _context.SaveChangesAsync();
            }
        }

        public async Task Remove(PendingLoginDto pendingLogin)
        {
            _context.PendingLogin.Remove(pendingLogin);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveOutdated()
        {
            List<PendingLoginDto> outdatedPendingLogin = await _context.PendingLogin
                .Where(pl => pl.ExpirationDate < DateTime.UtcNow)
                .ToListAsync();

            _context.RemoveRange(outdatedPendingLogin);
            await _context.SaveChangesAsync();
        }
    }
}
