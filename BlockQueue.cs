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

        // preview of the next coming blocks
        public List<Block> NextBlocks { get; private set; }

        // picks a random block
        private Block RandomBlock()
        {
            return blocks[random.Next(blocks.Length)];
        }

        public BlockQueue()
        {
            NextBlocks = new List<Block>();

            // generate first 3 blocks to queue
            for (int i = 0; i < 3; i++)
            {
                NextBlocks.Add(RandomBlock());
            }
        }

        // returns the next block (and updates it with a new one)
        public Block GetAndUpdate()
        {
            Block block = NextBlocks[0];
            NextBlocks.RemoveAt(0);
            NextBlocks.Add(RandomBlock());

            // pick a new block different from previous
            do
            {
                NextBlocks.RemoveAt(2);
                NextBlocks.Add(RandomBlock());
            }
            while(NextBlocks[1].Id == NextBlocks[2].Id);

            return block;
        }
    }
}
