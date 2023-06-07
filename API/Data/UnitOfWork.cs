using API.Interfaces;

namespace API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        public IUserRepository UserRepository => throw new NotImplementedException();

        public IMessageRepository MessageRepository => throw new NotImplementedException();

        public ILikesRepository LikesRepository => throw new NotImplementedException();

        public Task<bool> Complete()
        {
            throw new NotImplementedException();
        }

        public bool HasChanges()
        {
            throw new NotImplementedException();
        }
    }
}
