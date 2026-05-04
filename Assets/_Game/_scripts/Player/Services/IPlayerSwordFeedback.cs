namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Simple prototype swing motion (no Animator).
    /// </summary>
    public interface IPlayerSwordFeedback
    {
        /// <param name="comboIndex">1-based combo step index.</param>
        void PlaySwing(int comboIndex);
    }
}
