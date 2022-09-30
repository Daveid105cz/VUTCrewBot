using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.DAL.Entities;

namespace VUTCrewBot.DAL
{
    public class BotDbContext:DbContext
    {

        public DbSet<KeyValueSettingEntity> KeyValueSettings { get; set; }
        public DbSet<MeetEntity> Meets { get; set; }
        public DbSet<UserMeetResponse> Responses { get; set; }
        public BotDbContext(DbContextOptions contextOptions) : base(contextOptions) { }

        public override void Dispose()
        {
            base.Dispose();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var ulongConv = new ValueConverter<ulong, long>(
                v => ToLong(v),
                v => ToUlong(v));
            var enumConv = new ValueConverter<UserResponse, byte>(
                v => ToByte(v),
                v => ToUserResponse(v));

            modelBuilder
                .Entity<UserMeetResponse>()
                .Property(e => e.Response)
                .HasConversion(enumConv)
                .HasDefaultValue(UserResponse.None);

            modelBuilder.Entity<UserMeetResponse>()
                .HasKey(p => new { p.MeetId, p.UserId });


            modelBuilder
                .Entity<UserMeetResponse>()
                .Property(e => e.UserId)
                .HasConversion(ulongConv)
                .ValueGeneratedNever();
     
            //modelBuilder
            //    .Entity<ChannelEntity>()
            //    .Property(e => e.Id)
            //    .HasConversion(converter)
            //    .ValueGeneratedNever();
            //modelBuilder
            //    .Entity<ChannelEntity>()
            //    .Property(e => e.ServerId)
            //    .HasConversion(converter);

            //modelBuilder
            //    .Entity<ImageHashEntity>()
            //    .Property(e => e.MessageId)
            //    .HasConversion(converter);
            //modelBuilder
            //    .Entity<ImageHashEntity>()
            //    .Property(e => e.ChannelId)
            //    .HasConversion(converter);
            //modelBuilder
            //    .Entity<ImageHashEntity>()
            //    .Property(e => e.CalculatedDHash)
            //    .HasConversion(converter);
        }

        private static long ToLong(ulong value)
        {
            unchecked
            {
                long result = (long)value;
                return result;
            }
        }
        private static ulong ToUlong(long value)
        {
            unchecked
            {
                ulong result = (ulong)value;
                return result;
            }
        }
        private static UserResponse ToUserResponse(byte byt)
        {
            return ((byt == 1) ? UserResponse.Acked : ((byt == 2) ? UserResponse.Refused : UserResponse.None));
        }
        private static byte ToByte(UserResponse ur)
        {
            return (byte)((ur == UserResponse.None) ? 0 : ((ur == UserResponse.Acked) ? 1 : 2));
        }
    }
}
