using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Events;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class LibraryService (IUnitOfWork unitOfWork, IMapper mapper, IOrderPlacedProducer orderPlacedProducer) : ILibrarySevice
    {
        public async Task OrderPaymentProcessed(PaymentProcessedEvent message)
        {

            if (message.Status == "Approved")
            {
                var library =  new Library
                {
                    IDUsuario = message.UserId,
                    IDGame = message.GameId
                };

                var createLibrary = await unitOfWork.Library.AdicionarAsync(library);

                await unitOfWork.SaveChangesAsync();
            }
        }

        public async Task CriarLibraryAsync(LibraryRequest library)
        {            

            var game = await unitOfWork.Games.ObterPorIdAsync(library.IDGame) ?? throw new DirectoryNotFoundException(library.IDGame.ToString());

            var evento = new OrderPlacedEvent
            {
                UserId= library.IDUsuario,
                GameId = library.IDGame,
                Price = game.Price
            };

            await orderPlacedProducer.OrderPlacedPublish(evento);


        }

        public async Task DeletarAsync(int id)
        {
            var Library = await unitOfWork.Library.ObterPorIdAsync(id) ?? throw new DirectoryNotFoundException(id.ToString());

            await unitOfWork.Library.DeletarAsync(Library);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task<Library> ObterPorIdAsync(int id)
        {
            var Library = await unitOfWork.Library.ObterPorIdAsync(id) ?? throw new DirectoryNotFoundException(id.ToString());

            await unitOfWork.Library.DeletarAsync(Library);

            await unitOfWork.SaveChangesAsync();

            return Library;
        }

        public async Task<IEnumerable<Library>> ObterTodosAsync()
        {
            return await unitOfWork.Library.ObterTodosAsync();
        }
    }
}
