using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tetris
{
    public class BlockQueue
    {
        // available blocks to pick from
        private readonly Block[] blocks = new Block[]
        {
            new IBlock(),
            new JBlock(),
            new LBlock(),
            new OBlock(),
            new SBlock(),
            new TBlock(),
            new ZBlock()
        };

        private readonly Random random = new Random(Guid.NewGuid().GetHashCode());  // more random board for 2 players

        // preview of the next coming block
        public Block NextBlock { get; private set; }        // TODO: replace with an array (and then display it)

        // picks a random block
        private Block RandomBlock()
        {
            return blocks[random.Next(blocks.Length)];
        }

        public BlockQueue()
        {
            NextBlock = RandomBlock();
        }

        // returns the next block (and updates it with a new one)
        public Block GetAndUpdate()
        {
            Block block = NextBlock;

            // pick a new block different from previous
            do
            {
                NextBlock = RandomBlock();
            }
            while(block.Id == NextBlock.Id);

            return block;
        }
    }
}
