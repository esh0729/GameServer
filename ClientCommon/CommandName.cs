namespace ClientCommon
{
    public enum CommandName : int
    {
        None = 0,

        //
        // 계정
        //

        Login,
        LobbyInfo,

        //
        // 영웅
        //

        HeroCreate,
        HeroLogin,
        HeroLogout,
        HeroInitEnter,

        HeroMoveStart,
        HeroMove,
        HeroMoveEnd,

        //
        // 행동
        //

        Action
    }
}
