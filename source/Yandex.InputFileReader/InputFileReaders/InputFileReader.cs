using System;
using Yandex.Utils;

namespace Yandex.InputFileReader
{
    public abstract class InputFileReader : IDisposable
    {
        public virtual void Dispose()
        {
        }

        public virtual void onBeginRead()
        {
        }

        public virtual void onMetadata(Metadata metadata)
        {
        }

        public virtual void onQueryAction(QueryAction queryAction)
        {
        }

        public virtual void onClick(Click click)
        {
        }

        public virtual void onEndRead()
        {
        }
    }
}