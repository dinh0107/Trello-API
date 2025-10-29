using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trello_API.Models;

namespace Trello_API.DAL
{
    public class UnitOfWork : IDisposable
    {
        private readonly DataEntities _context = new DataEntities();
        private GenericRepository<User> _userRepository;
        private GenericRepository<Board> _boardRepository;
        private GenericRepository<Card> _cardRepository;
        private GenericRepository<List> _listRepository;
        private GenericRepository<Comment> _commentRepository;
        private GenericRepository<CardStatus> _cardStatusRepository;
        private GenericRepository<BoardUser> _boardUserRepository;
        public GenericRepository<BoardUser> BoardUserRepository =>
            _boardUserRepository ?? (_boardUserRepository = new GenericRepository<BoardUser>(_context));
        public GenericRepository<User> UserRepository =>
            _userRepository ?? (_userRepository = new GenericRepository<User>(_context));
        public GenericRepository<Board> BoardRepository =>
            _boardRepository ?? (_boardRepository = new GenericRepository<Board>(_context));
        public GenericRepository<Card> CardRepository =>
            _cardRepository ?? (_cardRepository = new GenericRepository<Card>(_context));
        public GenericRepository<List> ListRepository =>
            _listRepository ?? (_listRepository = new GenericRepository<List>(_context));
        public GenericRepository<Comment> CommentRepository =>
            _commentRepository ?? (_commentRepository = new GenericRepository<Comment>(_context));

        public GenericRepository<CardStatus> CardStatusRepository =>
           _cardStatusRepository ?? (_cardStatusRepository = new GenericRepository<CardStatus>(_context));
        public void Save()
        {
            _context.SaveChanges();
        }
        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}