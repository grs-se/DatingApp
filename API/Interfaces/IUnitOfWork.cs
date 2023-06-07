namespace API.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }
        IMessageRepository MessageRepository { get; }
        ILikesRepository LikesRepository { get; }
        Task<bool> Complete();
        // Tells us if Entity FW is tracking anything that is being changed inside its transaction.
        bool HasChanges();
    }
}
