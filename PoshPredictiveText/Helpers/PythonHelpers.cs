// TODO [ ][PYTHON] Create helper to parse .py file for arguments.
// Python has a standard library to define arguments, this knowledge can be
// used to parse python files to identify arguments for predictive completion.
// https://docs.python.org/3/library/argparse.html
/*
import argparse

parser = argparse.ArgumentParser(description='Process some integers.')
parser.add_argument('integers', metavar='N', type=int, nargs='+',
                    help='an integer for the accumulator')
parser.add_argument('--sum', dest='accumulate', action='store_const',
                    const=sum, default=max,
                    help='sum the integers (default: find the max)')

args = parser.parse_args()
print(args.accumulate(args.integers))
*/


namespace PoshPredictiveText.Helpers
{
    internal class PythonHelpers
    {
    }
}
